using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using TelePsy.BLL.Services;
using TelePsy.DAL.Repositories;
using TelePsy.Domain.Entities;
using TelePsy.Domain.Enums;
using Xunit;

namespace TelePsy.Tests.Services
{
    public class InvoiceServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly InvoiceService _service;

         public InvoiceServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _service = new InvoiceService(_unitOfWorkMock.Object);
        }

        [Fact]
        public async Task GeneratePatientInvoiceAsync_ValidPayment_CreatesInvoiceAndLinksToPayment()
        {
            // Arrange
            int paymentId = 1;
            var payment = new Payment
            {
                Id = paymentId,
                Amount = 150000,
                AppointmentId = 10,
                PatientInvoiceId = null
            };

            var appointment = new Appointment
            {
                Id = 10,
                PatientId = 5
            };

            var paymentRepoMock = new Mock<IGenericRepository<Payment>>();
            paymentRepoMock.Setup(r => r.GetByIdAsync(paymentId)).ReturnsAsync(payment);

            var appointmentRepoMock = new Mock<IGenericRepository<Appointment>>();
            appointmentRepoMock.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(appointment);

            var invoiceRepoMock = new Mock<IGenericRepository<Invoice>>();

            _unitOfWorkMock.Setup(u => u.Repository<Payment>()).Returns(paymentRepoMock.Object);
            _unitOfWorkMock.Setup(u => u.Repository<Appointment>()).Returns(appointmentRepoMock.Object);
            _unitOfWorkMock.Setup(u => u.Repository<Invoice>()).Returns(invoiceRepoMock.Object);

            // Act
            var result = await _service.GeneratePatientInvoiceAsync(paymentId);

            // Assert
            result.Should().NotBeNull();
            result.TotalAmount.Should().Be(150000);
            result.PatientId.Should().Be(5);
            result.Type.Should().Be(InvoiceType.ClientPurchase);
            
            invoiceRepoMock.Verify(r => r.AddAsync(It.IsAny<Invoice>()), Times.Once);
            paymentRepoMock.Verify(r => r.Update(payment), Times.Once);
            _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Exactly(2));
            payment.PatientInvoiceId.Should().NotBeNull();
        }

        [Fact]
        public async Task GeneratePsychologistPayoutAsync_ValidAppointments_CalculatesCorrectPayout()
        {
            // Arrange
            int psychologistId = 1;
            var apptIds = new List<int> { 1, 2 };
            
            var psychologistAppointments = new List<Appointment>
            {
                new Appointment { Id = 1, PsychologistId = psychologistId, Payment = new Payment { Amount = 100000 } },
                new Appointment { Id = 2, PsychologistId = psychologistId, Payment = new Payment { Amount = 200000 } }
            };

            var configData = new GlobalConfiguration { Key = "CommissionRate", Value = "0.20" };

            var apptRepoMock = new Mock<IGenericRepository<Appointment>>();
            apptRepoMock.Setup(r => r.GetAsync(
                It.IsAny<Expression<Func<Appointment, bool>>>(), 
                It.IsAny<Func<IQueryable<Appointment>, IOrderedQueryable<Appointment>>>(), 
                It.IsAny<string>()))
                .ReturnsAsync(() => new List<Appointment>(psychologistAppointments));
            
            var configRepoMock = new Mock<IGenericRepository<GlobalConfiguration>>();
            configRepoMock.Setup(r => r.GetAsync(
                It.IsAny<Expression<Func<GlobalConfiguration, bool>>>(), 
                It.IsAny<Func<IQueryable<GlobalConfiguration>, IOrderedQueryable<GlobalConfiguration>>>(), 
                It.IsAny<string>()))
                .ReturnsAsync(new List<GlobalConfiguration> { configData });

            var invoiceRepoMock = new Mock<IGenericRepository<Invoice>>();

            _unitOfWorkMock.Setup(u => u.Repository<Appointment>()).Returns(apptRepoMock.Object);
            _unitOfWorkMock.Setup(u => u.Repository<GlobalConfiguration>()).Returns(configRepoMock.Object);
            _unitOfWorkMock.Setup(u => u.Repository<Invoice>()).Returns(invoiceRepoMock.Object);

            // Act
            var result = await _service.GeneratePsychologistPayoutAsync(psychologistId, apptIds);

            // Assert
            result.Should().NotBeNull();
            result.TotalAmount.Should().Be(240000);
            result.Details.Count.Should().Be(2);
            result.Type.Should().Be(InvoiceType.PsychologistPayout);
            
            _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
        }
    }
}
