# UI Redesign Alignment Guidelines

Date: 2026-04-09
Applies to: All pages except login.

## 1. Layout Structure Standard

Every page should follow this structure:

1. page container
2. page header (title + subtitle + actions)
3. content sections/cards

Required:
1. Use one top-level container per page.
2. Keep section spacing consistent (16-24px desktop, 12-16px mobile).
3. Keep page max width constrained through the shell.

## 2. Spacing System

Use spacing tokens consistently:

1. xs: 4px
2. sm: 8px
3. md: 12px
4. lg: 16px
5. xl: 20px
6. xxl: 24px

Rules:
1. Component internal spacing: sm-lg
2. Between sibling cards/sections: lg-xl
3. Between page header and first content block: xl

## 3. Typography Rules

1. Page titles: 1.2rem-1.85rem, weight 760
2. Section titles: 1.0rem-1.25rem, weight 700+
3. Body text: 0.88rem-0.98rem
4. Meta/helper text: 0.74rem-0.84rem

Do:
1. Keep title hierarchy strict.
2. Keep subtitles muted and concise.

Avoid:
1. Multiple title sizes within same hierarchy level.
2. Uppercase body copy for long text.

## 4. Color and Surface Rules

Primary palette:

1. Primary: #1E3A5F
2. Accent: #22B8A8
3. Highlight: #F4A261
4. Surface: #FFFFFF
5. Background: #EEF3F8
6. Text: #132238
7. Muted text: #5F7288

Usage guidance:

1. Primary: headings, key actions, key labels
2. Accent: active/focus, secondary emphasis, progress
3. Highlight: warnings and KPI highlights
4. Danger/success/warning colors: status only

## 5. Grid and Responsiveness

Breakpoints:

1. >=1280px desktop wide
2. 1024-1279px desktop
3. 768-1023px tablet
4. 480-767px mobile
5. <480px compact mobile

Rules:

1. Forms: 2 columns desktop/tablet, 1 column mobile
2. KPI cards: 4-up desktop, 2-up tablet, 1-up mobile
3. Toolbars: wrap naturally; no clipped controls
4. Tables: preserve readability via horizontal scroll where needed

## 6. Forms and Inputs

1. Consistent control height and radius.
2. Label above input in narrow screens.
3. Group related fields in visual panels.
4. Always provide clear primary and secondary actions.

Validation and states:

1. Error text visible and close to field.
2. Focus state clearly visible (ring).
3. Disabled controls visibly distinct.

## 7. Data Grid Standards

1. Keep table inside card/panel shell.
2. Header row must remain legible and concise.
3. Row actions aligned consistently (typically trailing column).
4. Filters/search should be in toolbar, not scattered.

Mobile handling:

1. Allow horizontal scroll for dense tables.
2. Keep critical columns first.

## 8. Dashboard Standards

1. Keep KPI cards visually balanced.
2. Chart cards require title, filter area (if needed), content area, optional footer metrics.
3. Quick actions should be equally sized and tap-friendly.
4. Activity lists should keep icon + text alignment and readable timestamps.

## 9. Dialog and Modal Standards

1. Fixed spacing rhythm in header/content/footer.
2. Minimum touch target for action buttons.
3. Ensure dialogs remain usable at 360px width.
4. Avoid overflow clipping for dropdowns and date pickers.

## 10. Accessibility and Interaction

1. Keyboard navigation must reach all primary actions.
2. Visible focus indicator on all interactive controls.
3. Color contrast sufficient for text and buttons.
4. Respect reduced-motion preference.

## 11. Implementation Checklist (Per Page)

1. Page container and header pattern applied.
2. Spacing follows token system.
3. Typography hierarchy correct.
4. Color usage follows palette roles.
5. Form/grid components responsive.
6. No horizontal overflow at 360px.
7. Keyboard and focus behavior verified.

## 12. QA Matrix

For each redesigned page test:

1. Desktop wide (>=1280)
2. Desktop (1024)
3. Tablet (768)
4. Mobile (480)
5. Compact mobile (360)

Role checks:

1. Admin
2. IT
3. PDR
4. Purchasing
5. Infrastructure
6. Employee
