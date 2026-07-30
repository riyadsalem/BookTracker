# Book Tracker Evaluation

## Introduction

I looked at the application from both the USER and DEVELOPER side.

---

## Findings

| Improvement                            | Value  | Effort |  Risk  | Priority |
| -------------------------------------- | :----: | :----: | :----: | :------: |
| Add rate limiting to login             |  High  |  Low   |  High  |   High   |
| Add database indexes for search        |  High  |  Low   | Medium |   High   |
| Add frontend tests                     |  High  |  High  | Medium |  Medium  |
| Add optimistic concurrency for Members | Medium | Medium | Medium |  Medium  |
| Add refresh tokens                     | Medium | Medium |  Low   |  Medium  |
| Add API health check                   | Medium |  Low   |  Low   |   Low    |
| Add email verification                 | Medium | Medium | Medium |  Medium  |
| Store JWT in HttpOnly cookies          | Medium |  High  | Medium |  Medium  |
| Add soft delete and audit trail        |  Low   | Medium | Medium |   Low    |

---

## Scope

I focused on security, performance and code quality.....

---

## Expected Result

### 1. Add rate limiting to login

Protect the login endpoint against brute-force attacks by limiting repeated login attempts.

### 2. Add database indexes

Keep search fast, even when the number of books grows... (Book.Title & Book.Author)

### 3. Add frontend tests

Detect frontend bugs earlier and make future changes safer..... (This is a bit difficult because it requires precision and a lot of effort)

### 4. Add optimistic concurrency for Members

### 5. Add refresh tokens

Allow users to stay logged in without logging in again too often.

### 6. Add email verification

After registration, send a verification email with an activation link. This helps reduce fake accounts.

### 7. Store JWT in HttpOnly cookies

The JWT is currently stored in (localStorage). Using HttpOnly cookies would make it harder for XSS (Cross-Site Scripting) attacks to steal the token.
van (Application >>> Local Storage) Tot (Application >>> Cookies).

### 8. Add API health check

Add a `/health` endpoint to check if the API and database are running correctly. This helps monitor the application.

### 9. Add soft delete and audit trail

Allow deleted data to be recovered and keep a history of important changes.

---

## Suggested Order

1. Add rate limiting to login.
2. Add database indexes.
3. Add frontend tests.
4. Add optimistic concurrency for Members.
5. Add refresh tokens.
6. Add email verification.
7. Store JWT in HttpOnly cookies.
8. Add API health check.
9. Add soft delete and audit trail.

---

## Why This Order

I gave higher priority to improvements that provide high value with low effort or reduce security risks.
