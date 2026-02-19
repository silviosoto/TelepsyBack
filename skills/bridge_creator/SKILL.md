---
name: Bridge Creator (Next.js)
description: A skill focused on creating robust frontend-backend integrations in Next.js applications, emphasizing API abstraction, type safety, and custom hooks.
---

# Bridge Creator Skill: Next.js Frontend Integration

This skill guides the implementation of a professional integration layer between a Next.js frontend and a .NET backend.

## 1. API Client Strategy
Use a dedicated API client (e.g., Axios instance or fetch wrapper) to centralize request logic, base URLs, and interceptors.

### Example: Typed Axios Client
```typescript
// lib/api-client.ts
import axios from 'axios';

export const apiClient = axios.create({
  baseURL: process.env.NEXT_PUBLIC_API_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

apiClient.interceptors.response.use(
  (response) => response.data,
  (error) => Promise.reject(error)
);
```

## 2. Strong Typing with DTOs
Define TypeScript interfaces that mirror your .NET DTOs to ensure type safety across the boundary.

```typescript
// types/api.ts
export interface UserDto {
  id: string;
  email: string;
  fullName: string;
}

export interface CreateUserRequest {
  email: string;
  fullName: string;
}
```

## 3. Data Fetching Hooks (SWR / React Query)
Avoid calling API directly in components. Encapsulate logic in custom hooks.

### Example: SWR Pattern
```typescript
// hooks/useUsers.ts
import useSWR from 'swr';
import { apiClient } from '@/lib/api-client';
import { UserDto } from '@/types/api';

const fetcher = (url: string) => apiClient.get<UserDto[]>(url);

export function useUsers() {
  const { data, error, isLoading } = useSWR('/users', fetcher);
  
  return {
    users: data,
    isLoading,
    isError: error
  };
}
```

## 4. Server Actions (App Router)
For mutations or sensitive data fetching, prefer Server Actions in Next.js 14+.

```typescript
// actions/user-actions.ts
'use server'

import { revalidatePath } from 'next/cache';

export async function createUser(formData: FormData) {
  const rawData = {
    email: formData.get('email'),
    // ...
  };
  
  // Call .NET backend securely
  const response = await fetch(`${process.env.API_URL}/users`, {
    method: 'POST',
    body: JSON.stringify(rawData),
    // ...
  });
  
  revalidatePath('/users');
  return response.json();
}
```

## Best Practices
- **Environment Variables**: Always use `process.env` for API URLs.
- **Error Handling**: Implement global error boundaries or toast notifications for API failures.
- **DTO Sync**: Ideally, use tools like Swagger Codegen or OpenApi Generator if the backend produces an OpenAPI spec.
