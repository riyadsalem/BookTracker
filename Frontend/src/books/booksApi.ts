import { apiRequest, apiRequestWithoutResponse } from "../api";
import type { PagedResult } from "../types";
import type {
  BookDetails,
  BookSummary,
  CreateBookRequest,
  CreateBookResponse,
  GetBooksRequest,
  UpdateBookRequest,
} from "./types";

export function getBooks(request: GetBooksRequest) {
  /*
  {
   page: 2,
   pageSize: 10,
   search: "react"
   }
   */
  const parameters = new URLSearchParams({
    // Create Query string
    page: request.page.toString(),
    pageSize: request.pageSize.toString(),
  });

  if (request.search) {
    parameters.set("search", request.search);
  }

  // parameters.toString() >>>> page=2&pageSize=10&search=react
  return apiRequest<PagedResult<BookSummary>>(
    `/books?${parameters.toString()}`,
  );
}

export function getBook(bookId: number) {
  return apiRequest<BookDetails>(`/books/${bookId}`);
}

export function createBook(request: CreateBookRequest) {
  return apiRequest<CreateBookResponse>("/books", {
    method: "POST",
    body: JSON.stringify(request),
  });
}

export function updateBook(bookId: number, request: UpdateBookRequest) {
  return apiRequestWithoutResponse(`/books/${bookId}`, {
    method: "PUT",
    body: JSON.stringify(request),
  });
}

export function deleteBook(bookId: number) {
  return apiRequestWithoutResponse(`/books/${bookId}`, {
    method: "DELETE",
  });
}
