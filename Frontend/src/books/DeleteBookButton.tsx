import { useState } from "react";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useNavigate } from "react-router-dom";
import { ApiError } from "../api";
import { useCurrentMember } from "../auth/useCurrentMember";
import { deleteBook } from "./booksApi";

type DeleteBookButtonProps = {
  bookId: number;
  title: string;
};

export function DeleteBookButton({ bookId, title }: DeleteBookButtonProps) {
  const [confirming, setConfirming] = useState(false);
  const currentMemberQuery = useCurrentMember();
  const queryClient = useQueryClient();
  const navigate = useNavigate();

  // DELETE van DB and the cache (localStorge)
  function leaveDeletedBook() {
    // Mark the books list as stale because one book has been deleted.
    // The cached list is no longer up to date.
    // refetchType: "none" means don't refetch now,
    // because we are about to navigate to the books page,
    // and that page will fetch the updated list if needed.
    queryClient.invalidateQueries({
      queryKey: ["books"],
      refetchType: "none",
    });

    // Remove the deleted book's detail query from the cache.
    // There is no reason to keep cached data for a book
    // that no longer exists.
    // exact: true ensures that only this specific query is removed.
    queryClient.removeQueries({
      queryKey: ["books", "detail", bookId],
      exact: true,
    });

    navigate("/books");
  }
  const deleteMutation = useMutation({
    mutationFn: () => deleteBook(bookId),
    onSuccess: leaveDeletedBook,
  });

  if (
    !currentMemberQuery.isSuccess ||
    currentMemberQuery.data.role !== "Administrator"
  ) {
    return null;
  }

  if (!confirming) {
    return (
      <button type="button" onClick={() => setConfirming(true)}>
        Delete book
      </button>
    );
  }

  const mutationStatus =
    deleteMutation.error instanceof ApiError
      ? deleteMutation.error.status
      : null;

  return (
    <section aria-labelledby="delete-book-heading">
      <h2 id="delete-book-heading">Delete {title}?</h2>
      <p>This action cannot be undone.</p>
      <button
        type="button"
        onClick={() => deleteMutation.mutate()}
        disabled={deleteMutation.isPending}
      >
        {deleteMutation.isPending ? "Deleting..." : "Yes, delete book"}
      </button>
      <button
        type="button"
        onClick={() => {
          deleteMutation.reset();
          setConfirming(false);
        }}
        disabled={deleteMutation.isPending}
      >
        Cancel
      </button>
      {mutationStatus === 401 && <p>Your login is missing or expired.</p>}
      {mutationStatus === 403 && <p>Only administrators can delete books.</p>}
      {mutationStatus === 404 && (
        <div>
          <p>This book no longer exists. It may already have been deleted.</p>
          <button type="button" onClick={leaveDeletedBook}>
            Back to books
          </button>
        </div>
      )}
      {deleteMutation.isError && mutationStatus === null && (
        <p>Could not delete the book.</p>
      )}
    </section>
  );
}
