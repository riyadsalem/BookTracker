export type PagedResult<T> = {
  // PagedResult<MemberSummary>
  items: T[];
  page: number;
  pageSize: number;
  totalItems: number;
  totalPages: number;
};
