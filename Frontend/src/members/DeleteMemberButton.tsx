import { useState } from "react";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useNavigate } from "react-router-dom";
import { ApiError } from "../api";
import { deleteMember } from "./membersApi";

type DeleteMemberButtonProps = {
  memberId: number;
  label: string;
};

export function DeleteMemberButton({
  memberId,
  label,
}: DeleteMemberButtonProps) {
  const [confirming, setConfirming] = useState(false);
  const queryClient = useQueryClient();
  const navigate = useNavigate();

  function leaveDeletedMember() {
    queryClient.invalidateQueries({
      queryKey: ["members"],
      refetchType: "none",
    });
    queryClient.removeQueries({
      queryKey: ["members", "detail", memberId],
      exact: true,
    });
    navigate("/members");
  }

  const deleteMutation = useMutation({
    mutationFn: () => deleteMember(memberId),
    onSuccess: leaveDeletedMember,
  });

  if (!confirming) {
    return (
      <button type="button" onClick={() => setConfirming(true)}>
        Delete member
      </button>
    );
  }

  const mutationStatus =
    deleteMutation.error instanceof ApiError
      ? deleteMutation.error.status
      : null;

  return (
    <section aria-labelledby="delete-member-heading">
      <h2 id="delete-member-heading">Delete {label}?</h2>
      <p>This action cannot be undone.</p>
      <button
        type="button"
        onClick={() => deleteMutation.mutate()}
        disabled={deleteMutation.isPending}
      >
        {deleteMutation.isPending ? "Deleting..." : "Yes, delete member"}
      </button>{" "}
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
      {mutationStatus === 403 && (
        <p>Only administrators can delete other members.</p>
      )}
      {mutationStatus === 404 && (
        <div>
          <p>This member no longer exists. It may already have been deleted.</p>
          <button type="button" onClick={leaveDeletedMember}>
            Back to members
          </button>
        </div>
      )}
      {deleteMutation.isError && mutationStatus === null && (
        <p>Could not delete the member.</p>
      )}
    </section>
  );
}
