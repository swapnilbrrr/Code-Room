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
- [ ] A fresh MySQL database can be prepared from the submitted project without the developer's personal database.
- [ ] Responsive UI works on desktop and mobile.
- [ ] Security basics are implemented: password hashing, authorization, validation, safe configuration, anti-forgery protection where applicable.
- [ ] Automated tests and build verification pass.
- [ ] A final manual test pass covers the main user journeys.
- [ ] README contains setup/run instructions suitable for the group and marker.

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
- [x] Database-backed course, lesson, quiz and student flows implemented.
- [ ] Replace remaining static/demo content with database-backed content where required.

## Phase 2 — Database & Persistence

- [x] Remove committed database credentials.
- [x] Add local user-secrets support.
- [x] Add reproducible MySQL database setup script.
- [x] Configure application startup to require a valid database connection.
- [x] Seed demo users, courses, lessons, quizzes, resources and announcements.
- [ ] Verify relationships and constraints against a clean MySQL database.
- [ ] Verify clean database setup from scratch on the development laptop.
- [ ] Verify the full database setup from the final submission ZIP on a second environment.

## Phase 3 — Authentication & Authorization

- [x] Registration flow.
- [x] Secure password hashing.
- [x] Login/logout flow.
- [x] Cookie authentication.
- [x] Student authorization.
- [x] Admin authorization.
- [x] Unauthorized/forbidden handling.
- [x] Validation and anti-forgery protection.

## Phase 4 — Core Learning Platform

- [x] Course CRUD.
- [x] Lesson CRUD.
- [x] Resource management.
- [x] Course enrollment.
- [x] Lesson completion/progress.
- [x] Quiz/question CRUD.
- [x] Quiz submission and scoring.
- [x] Quiz attempt history.
- [x] Dashboard statistics.
- [x] Announcements.
- [ ] Verify each major CRUD workflow manually.

## Phase 5 — Added Features (Scope Controlled)

- [x] Course search.
- [x] Category/level filtering.
- [x] Progress indicators.
- [x] Quiz history.
- [ ] Resource/download experience for students.
- [ ] Student dashboard analytics.
- [x] Dark/light theme toggle.
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
- [x] Remove committed development database secrets.
- [ ] Automated build and test verification passes.
- [ ] Final clean clone setup verified.
- [ ] Final submission ZIP tested from scratch.
- [ ] Submission-ready README.

## Status Legend

- [x] Complete
- [ ] Not complete
- BLOCKED = waiting on a dependency
- POLISH = functional but still needs visual/UX refinement

Rule: once every mandatory item is checked and the selected added-feature scope is reasonably polished, we stop. We do not keep adding features just because we can.
