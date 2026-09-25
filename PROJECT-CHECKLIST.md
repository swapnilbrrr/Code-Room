# Code-Room Completion Checklist

This is the project control document. We are aiming for complete and polished, not endless perfection.

## Definition of Done

The project is submission-ready when:
- [ ] Every mandatory proposal requirement is implemented and tested.
- [ ] Student registration/login and authenticated member flow work.
- [ ] Course/lesson/resource/quiz/progress functionality works end-to-end.
- [ ] Admin functionality supports required CRUD and content management.
- [ ] Client-side and server-side validation are present on relevant forms.
- [ ] MySQL + Entity Framework Core persistence works from a clean setup.
- [ ] Responsive UI works on desktop and mobile.
- [ ] Security basics are implemented: password hashing, authorization, validation, safe configuration, anti-forgery protection where applicable.
- [ ] Automated tests and build verification pass.
- [ ] A final manual test pass covers the main user journeys.
- [ ] README contains setup/run instructions suitable for the group.

## Phase 0 — Project Foundation

- [x] GitHub repository created.
- [x] .NET 8 solution created.
- [x] ASP.NET Core MVC project created.
- [x] MySQL/EF Core packages added.
- [x] Initial DbContext created.
- [x] Core domain models scaffolded.
- [x] Dev Container configuration added.

## Phase 1 — Frontend Skeleton

- [x] Global layout/navbar/footer.
- [x] Initial Code-Room visual identity.
- [x] Responsive base styling.
- [x] Home page.
- [x] Course listing route/page.
- [x] Course details route/page.
- [x] Lesson route/page.
- [x] Login page.
- [x] Registration page.
- [x] Student dashboard page.
- [x] Quiz page.
- [x] Quiz results page.
- [x] Admin dashboard page.
- [x] About page.
- [x] FAQ page.
- [ ] Replace remaining static/demo content with database-backed content.

## Phase 2 — Database & Persistence

- [ ] Configure MySQL safely through environment/user secrets.
- [ ] Create EF Core migrations.
- [ ] Create database.
- [ ] Seed realistic demo courses, lessons, quizzes and resources.
- [ ] Verify relationships and constraints.
- [ ] Verify clean database setup from scratch.

## Phase 3 — Authentication & Authorization

- [ ] Registration flow.
- [ ] Secure password hashing.
- [ ] Login/logout flow.
- [ ] Session/cookie authentication.
- [ ] Student authorization.
- [ ] Admin authorization.
- [ ] Unauthorized/forbidden handling.
- [ ] Validation and anti-forgery protection.

## Phase 4 — Core Learning Platform

- [ ] Course CRUD.
- [ ] Lesson CRUD.
- [ ] Resource management.
- [ ] Course enrollment.
- [ ] Lesson completion/progress.
- [ ] Course progress calculation.
- [ ] Quiz/question CRUD.
- [ ] Quiz submission and scoring.
- [ ] Quiz attempt history.
- [ ] Dashboard statistics.
- [ ] Announcements.

## Phase 5 — Added Features (Scope Controlled)

- [ ] Course search.
- [ ] Category/level filtering.
- [ ] Progress indicators.
- [ ] Quiz history.
- [ ] Resource/download section.
- [ ] Student dashboard analytics.
- [ ] Dark/light theme toggle.
- [ ] Responsive/mobile polish.

Optional only if all required functionality is stable:
- [ ] Bookmarks/favourites.
- [ ] Learning streak.
- [ ] Certificate-style completion page.
- [ ] Richer admin analytics.

## Phase 6 — Quality & Submission

- [ ] Validation test pass.
- [ ] Authentication/authorization test pass.
- [ ] CRUD test pass.
- [ ] Quiz/progress test pass.
- [ ] Responsive UI test pass.
- [ ] Accessibility/basic usability review.
- [ ] Remove placeholder links/content.
- [ ] Remove development secrets.
- [ ] Automated build and test verification passes.
- [ ] Final clean clone/Codespaces setup verified.
- [ ] Submission-ready README.

## Status Legend

- [x] Complete
- [ ] Not complete
- BLOCKED = waiting on a dependency
- POLISH = functional but still needs visual/UX refinement

Rule: once every mandatory item is checked and the selected added-feature scope is reasonably polished, we stop. We do not keep adding features just because we can.
