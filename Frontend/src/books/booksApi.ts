import { apiRequest } from "../api";
import type { PagedResult } from "../types";
import type { BookDetails, BookSummary, GetBooksRequest } from "./types";

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
