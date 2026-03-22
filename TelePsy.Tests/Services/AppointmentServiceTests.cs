using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using TelePsy.BLL.Interfaces;
using TelePsy.BLL.Services;
using TelePsy.DAL.Repositories;
using TelePsy.Domain.Entities;
using TelePsy.Domain.Enums;
using TelePsy.Domain.DTOs;
using Xunit;

namespace TelePsy.Tests.Services
{
    public class AppointmentServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IEmailService> _emailServiceMock;
        private readonly AppointmentService _service;

        public AppointmentServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _emailServiceMock = new Mock<IEmailService>();
            _service = new AppointmentService(_unitOfWorkMock.Object, _emailServiceMock.Object);
        }

        [Fact]
        public async Task CreateAppointmentAsync_WithValidTherapy_SetsRateFromTherapy()
        {
            // Arrange
            var appointment = new Appointment
            {
                PsychologistId = 1,
                TherapyId = 1,
                ScheduledTime = DateTime.UtcNow.AddDays(1)
            };

            var therapyConfig = new PsychologistTherapy
            {
                PsychologistId = 1,
                TherapyId = 1,
                Rate = 150000,
                IsActive = true
            };

            var therapyRepoMock = new Mock<IGenericRepository<PsychologistTherapy>>();
            therapyRepoMock.Setup(r => r.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<PsychologistTherapy, bool>>>(),
                It.IsAny<string>()))
                .ReturnsAsync(therapyConfig);

            var appointmentRepoMock = new Mock<IGenericRepository<Appointment>>();

            _unitOfWorkMock.Setup(u => u.Repository<PsychologistTherapy>()).Returns(therapyRepoMock.Object);
            _unitOfWorkMock.Setup(u => u.Repository<Appointment>()).Returns(appointmentRepoMock.Object);

            // Act
            var result = await _service.CreateAppointmentAsync(appointment);

            // Assert
            result.Rate.Should().Be(150000);
            result.Status.Should().Be(AppointmentStatus.Pending);
            appointmentRepoMock.Verify(r => r.AddAsync(It.IsAny<Appointment>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateAppointmentAsync_WithInvalidTherapy_ThrowsException()
        {
            // Arrange
            var appointment = new Appointment
            {
                PsychologistId = 1,
                TherapyId = 1
            };

            var therapyRepoMock = new Mock<IGenericRepository<PsychologistTherapy>>();
            therapyRepoMock.Setup(r => r.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<PsychologistTherapy, bool>>>(),
                It.IsAny<string>()))
                .ReturnsAsync((PsychologistTherapy)null);

            _unitOfWorkMock.Setup(u => u.Repository<PsychologistTherapy>()).Returns(therapyRepoMock.Object);

            // Act
            Func<Task> act = async () => await _service.CreateAppointmentAsync(appointment);

            // Assert
            await act.Should().ThrowAsync<Exception>().WithMessage("Selected therapy is not available for this psychologist.");
        }

        [Fact]
        public async Task CancelAppointmentAsync_WithExistingAppointment_UpdatesStatusAndSendsEmail()
        {
            // Arrange
            int appointmentId = 1;
            var appointment = new Appointment
            {
                Id = appointmentId,
                Status = AppointmentStatus.Confirmed,
                Patient = new Patient { Person = new Person { User = new User { Email = "patient@test.com" } } },
                Psychologist = new Psychologist { Person = new Person { User = new User { Email = "psych@test.com" } } }
            };

            var appointmentRepoMock = new Mock<IGenericRepository<Appointment>>();
            appointmentRepoMock.Setup(r => r.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<Appointment, bool>>>(),
                It.IsAny<string>()))
                .ReturnsAsync(appointment);

            _unitOfWorkMock.Setup(u => u.Repository<Appointment>()).Returns(appointmentRepoMock.Object);

            // Act
            await _service.CancelAppointmentAsync(appointmentId);

            // Assert
            appointment.Status.Should().Be(AppointmentStatus.Cancelled);
            appointmentRepoMock.Verify(r => r.Update(appointment), Times.Once);
            _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
            _emailServiceMock.Verify(e => e.SendAppointmentChangeNotificationAsync(appointment, "Cita Cancelada", "System"), Times.Once);
        }

        [Fact]
        public async Task RescheduleAppointmentAsync_WithDateInMoreThanOneMonth_ThrowsException()
        {
            // Arrange
            var originalDate = DateTime.UtcNow.AddDays(1);
            var tooFarDate = originalDate.AddMonths(1).AddDays(1);
            var appointment = new Appointment { Id = 1, ScheduledTime = originalDate };

            var appointmentRepoMock = new Mock<IGenericRepository<Appointment>>();
            appointmentRepoMock.Setup(r => r.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<Appointment, bool>>>(),
                It.IsAny<string>()))
                .ReturnsAsync(appointment);

            _unitOfWorkMock.Setup(u => u.Repository<Appointment>()).Returns(appointmentRepoMock.Object);

            // Act
            Func<Task> act = async () => await _service.RescheduleAppointmentAsync(1, tooFarDate);

            // Assert
            await act.Should().ThrowAsync<Exception>().WithMessage("La nueva fecha no puede ser superior a un mes de la fecha original.");
        }

        [Fact]
        public async Task InitiateBookingAsync_ValidInputs_CreatesAppointmentInvoiceAndDetail()
        {
            // Arrange
            var userId = "user123";
            var dto = new InitiateBookingDto
            {
                PsychologistId = 1,
                TherapyId = 1,
                ScheduledTime = DateTime.UtcNow.AddDays(1)
            };

            var person = new Person { Id = 1, UserId = userId };
            var patient = new Patient { Id = 1, PersonId = 1 };
            var therapyConfig = new PsychologistTherapy { Rate = 150000 };

            var personRepoMock = new Mock<IGenericRepository<Person>>();
            personRepoMock.Setup(r => r.GetAsync(It.IsAny<Expression<Func<Person, bool>>>(), null, ""))
                .ReturnsAsync(new List<Person> { person });

            var patientRepoMock = new Mock<IGenericRepository<Patient>>();
            patientRepoMock.Setup(r => r.GetAsync(It.IsAny<Expression<Func<Patient, bool>>>(), null, ""))
                .ReturnsAsync(new List<Patient> { patient });

            var appointmentRepoMock = new Mock<IGenericRepository<Appointment>>();
            appointmentRepoMock.Setup(r => r.GetAsync(It.IsAny<Expression<Func<Appointment, bool>>>(), null, ""))
                .ReturnsAsync(new List<Appointment>()); // Not busy

            var therapyRepoMock = new Mock<IGenericRepository<PsychologistTherapy>>();
            therapyRepoMock.Setup(r => r.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<PsychologistTherapy, bool>>>(), ""))
                .ReturnsAsync(therapyConfig);

            var invoiceRepoMock = new Mock<IGenericRepository<Invoice>>();
            var detailRepoMock = new Mock<IGenericRepository<InvoiceDetail>>();

            _unitOfWorkMock.Setup(u => u.Repository<Person>()).Returns(personRepoMock.Object);
            _unitOfWorkMock.Setup(u => u.Repository<Patient>()).Returns(patientRepoMock.Object);
            _unitOfWorkMock.Setup(u => u.Repository<Appointment>()).Returns(appointmentRepoMock.Object);
            _unitOfWorkMock.Setup(u => u.Repository<PsychologistTherapy>()).Returns(therapyRepoMock.Object);
            _unitOfWorkMock.Setup(u => u.Repository<Invoice>()).Returns(invoiceRepoMock.Object);
            _unitOfWorkMock.Setup(u => u.Repository<InvoiceDetail>()).Returns(detailRepoMock.Object);

            // Act
            var result = await _service.InitiateBookingAsync(userId, dto);

            // Assert
            result.Should().NotBeNull();
            result.Message.Should().Be("Booking initiated successfully");
            
            appointmentRepoMock.Verify(r => r.AddAsync(It.IsAny<Appointment>()), Times.Once);
            invoiceRepoMock.Verify(r => r.AddAsync(It.IsAny<Invoice>()), Times.Once);
            detailRepoMock.Verify(r => r.AddAsync(It.IsAny<InvoiceDetail>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Exactly(3));
        }
    }
}

