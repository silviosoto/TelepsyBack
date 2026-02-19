---
name: Next.js Fetcher
description: Master data fetching in Next.js, including Server Components, Client-side fetching with SWR, caching strategies, and performance optimization patterns.
---

# Next.js Fetcher Skill: High-Performance Data Loading

This skill focuses on implementing modern data fetching patterns in Next.js, leveraging both Server and Client capabilities to ensure fast, type-safe, and reliable applications.

## 1. Server-Side Fetching (App Router)
Next.js extends the native `fetch` API to provide per-request caching and revalidation. Use async Server Components for data fetching whenever possible.

```typescript
// app/posts/page.tsx
async function getPosts() {
  const res = await fetch('https://api.example.com/posts', {
    next: { revalidate: 3600 }, // Cache for 1 hour
  });
  
  if (!res.ok) throw new Error('Failed to fetch posts');
  return res.json();
}

export default async function Page() {
  const posts = await getPosts();
  return (
    <ul>
      {posts.map((post) => (
        <li key={post.id}>{post.title}</li>
      ))}
    </ul>
  );
}
```

## 2. Dynamic Data and Tags
Use `tags` for on-demand revalidation and `no-store` for truly dynamic data.

```typescript
// Fetching with tags
const res = await fetch('https://api.example.com/data', { 
  next: { tags: ['collection'] } 
});

// Revalidating from a Server Action or Route Handler
import { revalidateTag } from 'next/cache';
revalidateTag('collection');
```

## 3. Client-Side Fetching (SWR)
For highly interactive parts of the UI (dashboards, user-specific data), use the SWR hook to benefit from caching, revalidation on focus, and optimistic updates.

```typescript
// components/Profile.tsx
'use client'

import useSWR from 'swr';

const fetcher = (url: string) => fetch(url).then(r => r.json());

export function Profile() {
  const { data, error, isLoading } = useSWR('/api/user', fetcher);

  if (error) return <div>Failed to load</div>;
  if (isLoading) return <div>Loading...</div>;
  return <div>Hello {data.name}!</div>;
}
```

## 4. Parallel vs Sequential Fetching
Optimize performance by initiating multiple fetches simultaneously when they don't depend on each other.

```typescript
// Parallel Fetching
async function Page({ params: { id } }) {
  // Initiate both requests in parallel
  const artistData = getArtist(id);
  const albumsData = getArtistAlbums(id);

  // Wait for both promises to resolve
  const [artist, albums] = await Promise.all([artistData, albumsData]);

  return (
    <>
      <h1>{artist.name}</h1>
      <AlbumsList list={albums} />
    </>
  );
}
```

## 5. Loading and Error Handling
Leverage `loading.tsx` and `error.tsx` for seamless user experiences during data transitions.

- `loading.tsx`: Automatically creates a Suspense boundary for the page content.
- `error.tsx`: Catch runtime errors and provide a fallback UI with a retry mechanism.

## Best Practices
- **Prefer Server Components**: Fetch data on the server to reduce client-side JavaScript and improve SEO.
- **Type Everything**: Use TypeScript interfaces for API responses to catch errors during development.
- **Limit Data**: Only fetch the fields needed by the UI to reduce payload size.
- **Use Streaming**: Break down slow fetches into smaller chunks using React Suspense boundaries.
