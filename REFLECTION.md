# React vs Blazor (my experience)

After building both frontends for Book Tracker, here are my main findings.

## What is the same

- **One place for the Bearer token.** In React this lives in `src/api.ts` (`apiRequest()`). In Blazor it lives in `Auth/AuthorizationMessageHandler.cs`. No page ever adds the header itself, in either case.
- **Route protection follows the same idea, different implementation.** React: a custom `RequireAdministrator` wrapper around `<Outlet />` (`src/auth/RequireAdministrator.tsx`). Blazor: the built-in `@attribute [Authorize(Roles = "Administrator")]`.
- **Both frontends treat expected failures (404, conflict) as normal outcomes, not crashes** ... though the way they do this is different (see below).

## What is Blazor-specific

- `EditForm` with `[Required]`/`[Compare]` gives free client-side validation straight from the model. React had no comparable built-in mechanism.
- `AuthorizeView` automatically reacts to login state as soon as `AuthenticationStateProvider` changes. In React I had to build this myself in `Navigation.tsx`, with a manual `role === "Administrator"` check.
- An explicit `Result` type per API call (`CreateBookResult`, `UpdateBookResult`) is a pattern I only used in Blazor. In React (`booksApi.ts`) I use an `ApiError` class with a `status` property instead, and each page catches that exception itself (`error.status === 400`). Blazor's approach (never throwing for an expected status) felt more explicit; React's approach needed less code per call.
- Lifecycle hooks (`OnInitializedAsync`, `OnParametersSetAsync`) are a different mental model than `useEffect` with a dependency array.

## Where state management felt clearer

React, thanks to TanStack Query. Example from `BookListPage.tsx`:

```ts
useQuery({ queryKey: ["books", { page, search }], queryFn: () => getBooks(...) })
```

Caching and "keep showing old data while reloading" (`keepPreviousData`) come for free. In Blazor (`MemberList.razor`, `EditBook.razor`) every page manages its own `loading`/`result`/`errorMessage` fields... it works fine, but there is more repetition per page.

## Where HTTP and caching felt clearer

React again. After a mutation in, for example, `CreateBookPage.tsx`:

```ts
await queryClient.invalidateQueries({ queryKey: ["books"] })
```

and every page using that same query key refreshes itself automatically. Blazor has no built-in cache: every navigation does a fresh request, and after saving I have to call `Load()` or `LoadBook()` again myself in the same component.

## What would I choose myself

React with TanStack Query, mainly because of the caching and invalidation that I had to repeat by hand in every Blazor page. Blazor's strong point was end-to-end type safety with C# on both sides of the API, and the explicit `Result` types that forced clear error handling.

---

There are more differences I didn't cover here....