# UI Redesign Roadmap (All Pages Except Login)

Date: 2026-04-09
Scope: Full application UI/UX refresh for dashboard, page layouts, spacing, responsiveness, and visual consistency.
Out of scope: Login page (already finalized).

## Goals

1. Create one visual system across all roles and workflows.
2. Improve readability and spacing hierarchy.
3. Ensure every view is fully responsive (desktop, tablet, mobile).
4. Keep Radzen components consistent via shared tokens and wrappers.
5. Minimize regressions by redesigning in controlled, testable batches.

## Success Criteria

1. Every page uses consistent header, content container, and section spacing.
2. No horizontal overflow on mobile widths >= 360px.
3. All major tables are usable on tablet and mobile (scroll or stacked strategy).
4. Color contrast and focus states are accessible for action controls.
5. Dashboard KPIs/charts retain alignment at all breakpoints.

## Redesign Phases

## Phase 1 - Foundation (Done / In Progress)

1. Introduce shared UI tokens and utility classes in a global stylesheet.
2. Align shell, content container, and sidebar responsive behavior.
3. Standardize card, panel, and form primitives.

Deliverables:
1. Global design-system CSS foundation.
2. Shared spacing and grid utilities.
3. Responsive shell behavior updates.

## Phase 2 - Dashboard Systemization

1. Normalize dashboard component spacing and card rhythm.
2. Standardize KPI card heights, typography, and icon treatment.
3. Improve chart and legend consistency.
4. Ensure quick actions and tables collapse correctly on small screens.

Target files:
1. Components/Pages/Index.razor
2. Components/Pages/Index.razor.css

## Phase 3 - Workflow Screens (Role-by-Role)

1. Materials
2. Assignments
3. Delivery Orders
4. Purchase + Pending Deliveries
5. Infrastructure
6. Lifecycle + Maintenance + Predictions
7. Admin CRUD pages

For each workflow:
1. Apply page container/header pattern.
2. Normalize toolbars (search/filter/actions).
3. Normalize forms (2-column desktop, 1-column mobile).
4. Normalize data-grid spacing and action placement.

## Phase 4 - Polish and Accessibility

1. Focus rings and keyboard target consistency.
2. Empty states and loading states alignment.
3. Dialog spacing + mobile fit validation.
4. Animation reduction compliance for reduced-motion users.

## Phase 5 - Validation and Signoff

1. Responsive sweep at 360px, 480px, 768px, 1024px, 1280px.
2. Role-based visual smoke tests (Admin, IT, PDR, Purchasing, Infrastructure, Employee).
3. Build/test validation after each redesign batch.
4. Final screenshot review checklist.

## Batch Execution Plan

1. Batch A: Dashboard + shared shell
2. Batch B: Materials + Assignments pages
3. Batch C: Delivery and Purchase flows
4. Batch D: Infra + Lifecycle + Maintenance + Predictions
5. Batch E: Admin CRUD and edge dialogs

Rules:
1. Complete one batch, run build/tests, then proceed.
2. Keep login untouched.
3. Avoid mixing behavioral refactors with visual refactors in the same commit.

## Risk Control

1. Keep CSS changes additive via shared tokens/utilities.
2. Prefer local page CSS updates only where behavior diverges.
3. Avoid broad selector overrides that may impact login/auth.
4. Validate overflow and wrapping after every batch.

## Tracking Checklist

1. Foundation styles loaded and stable.
2. Dashboard fully aligned and responsive.
3. All page headers standardized.
4. All form pages responsive and spacing-compliant.
5. All table pages responsive and action-consistent.
6. Role-specific pages reviewed.
7. Accessibility and focus checks passed.
8. Final signoff complete.

## Current Progress Snapshot

Completed in this batch:

1. Introduced shared list-page utilities in `ui-redesign-foundation.css`:
	- `ux-list-shell`
	- `ux-list-header`
	- `ux-list-title`
	- `ux-list-toolbar`
	- `ux-search-input`
	- `ux-data-grid`
2. Applied shared list-page pattern to:
	- Purchase Requests page
	- Pending Deliveries page
	- Suppliers page
3. Removed repeated inline styling from those pages in favor of reusable responsive classes.
4. Extended the shared list-page pattern to both materials overview surfaces:
	- MaterialsViewInterface
	- MaterialsViewInterfacePDR
