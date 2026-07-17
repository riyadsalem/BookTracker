import { useState } from "react";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useNavigate } from "react-router-dom";
import { ApiError } from "../api";
import { deleteMember } from "../members/membersApi";
import { removeAccessToken } from "./tokenStorage";

type DeleteAccountButtonProps = {
  memberId: number;
  label: string;
};

export function DeleteAccountButton({
  memberId,
  label,
}: DeleteAccountButtonProps) {
  const [confirming, setConfirming] = useState(false);
  const queryClient = useQueryClient();
  const navigate = useNavigate();

  const deleteMutation = useMutation({
    mutationFn: () => deleteMember(memberId),
    onSuccess: () => {
      removeAccessToken();
      queryClient.clear();
      navigate("/");
    },
  });

  if (!confirming) {
    return (
      <button type="button" onClick={() => setConfirming(true)}>
        Delete account
      </button>
    );
  }

  const mutationStatus =
    deleteMutation.error instanceof ApiError
      ? deleteMutation.error.status
      : null;

  return (
    <section aria-labelledby="delete-account-heading">
      <h2 id="delete-account-heading">Delete account ({label})?</h2>
      <p>This action cannot be undone. You will be logged out.</p>
      <button
        type="button"
        onClick={() => deleteMutation.mutate()}
        disabled={deleteMutation.isPending}
      >
        {deleteMutation.isPending ? "Deleting..." : "Yes, delete my account"}
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
        <p>You are not allowed to delete this account.</p>
      )}
      {mutationStatus === 404 && <p>This account no longer exists.</p>}
      {deleteMutation.isError && mutationStatus === null && (
        <p>Could not delete your account.</p>
      )}
    </section>
  );
}
