---
name: Premium UI (Design & Aesthetics)
description: Master the art of creating high-end, high-performance user interfaces in Next.js, focusing on aesthetics, micro-animations, and premium UX.
---

# Premium UI Skill: Stunning & High-End Interfaces

In a tele-psychology application, the UI must feel calm, professional, and trustworthy.

## 1. Professional Color Palettes
Avoid generic colors. Use harmonious, balanced palettes (e.g., Soft Blues, Sage Greens, and Warm Grays).

```css
/* index.css or tokens.css */
:root {
  --primary-calm: hsl(210, 20%, 30%);
  --accent-soft: hsl(160, 15%, 90%);
  --background-glass: rgba(255, 255, 255, 0.7);
  --glass-border: rgba(255, 255, 255, 0.2);
}
```

## 2. Glassmorphism & Depth
Implement subtle glassmorphism for cards and navigation bars to provide a modern, premium feel.

```css
.premium-card {
  background: var(--background-glass);
  backdrop-filter: blur(12px);
  border: 1px solid var(--glass-border);
  box-shadow: 0 8px 32px 0 rgba(0, 0, 0, 0.05);
  border-radius: 16px;
}
```

## 3. Micro-Animations (Framer Motion)
Use animations to guide focus and make the application feel "alive".

```typescript
// components/TherapistCard.tsx
import { motion } from 'framer-motion';

export const TherapistCard = ({ name }) => (
  <motion.div
    whileHover={{ y: -5, scale: 1.02 }}
    initial={{ opacity: 0, y: 20 }}
    animate={{ opacity: 1, y: 0 }}
    transition={{ duration: 0.5 }}
    className="premium-card"
  >
    <h3>{name}</h3>
  </motion.div>
);
```

## 4. Typography & Spacing
- **Font Selection**: Use Google Fonts like *Inter*, *Outfit*, or *Bormioli* for a professional yet approachable look.
- **Whitespace**: Don't be afraid of empty space. It reduces cognitive load, which is critical in a therapeutic context.
- **Hierarchy**: Clear distinction between headings, subheadings, and body text.

## 5. Responsive Design (Mobile First)
Ensure the therapist search and appointment booking work perfectly on mobile devices.

```css
@media (max-width: 768px) {
  .hero-section {
    padding: 2rem 1rem;
    font-size: 1.5rem;
  }
}
```

## Best Practices
- **Consistency**: Use a consistent spacing and color system (Design Tokens).
- **Accessibility (a11y)**: Ensure high contrast ratios and proper ARIA labels.
- **Performance**: High-end doesn't mean slow. Optimize images and use CSS-only effects when possible.
- **Visual Feedback**: Every interaction (click, hover) should have subtle visual feedback.
