import { useState, type FormEvent } from "react";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { Link, useNavigate } from "react-router-dom";
import { ApiError } from "../api";
import { updateMember } from "../members/membersApi";
import type { UpdateMemberRequest } from "../members/types";
import { useCurrentMember } from "./useCurrentMember";
import type { CurrentMember } from "./types";

/////// this page voooooooor USER..... (zijn account) {useCurrentMember data}
export function EditAccountPage() {
  const currentMemberQuery = useCurrentMember();
  const [formError, setFormError] = useState<string | null>(null);
  const queryClient = useQueryClient();
  const navigate = useNavigate();

  const updateMutation = useMutation({
    mutationFn: (request: UpdateMemberRequest) => {
      if (!currentMemberQuery.data) {
        throw new Error("Current member is not loaded");
      }

      return updateMember(currentMemberQuery.data.id, request);
    },
    onSuccess: (_, request) => {
      // QueryClient.setQueryData ==>> I already know the new data.
      // ==> Therefore, there's no need to request a new one. i wll change the cache directly.
      // Update the cache directly because we already know the new data.
      // This refreshes the UI immediately without making another API request.

      queryClient.setQueryData<CurrentMember>(["current-member"], (old) =>
        // misschen old is null of undefine.... DUS old ? ... ? old
        old ? { ...old, name: request.name, email: request.email } : old,
      );
      navigate("/account");
    },
  });

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setFormError(null);

    const formData = new FormData(event.currentTarget);
    const name = formData.get("name")?.toString().trim() ?? "";
    const email = formData.get("email")?.toString().trim() ?? "";

    if (!name || !email) {
      setFormError("Enter a name and email.");
      return;
    }

    updateMutation.mutate({ name, email });
  }

  if (currentMemberQuery.isPending) {
    return <p>Loading account...</p>;
  }

  if (currentMemberQuery.isError || !currentMemberQuery.data) {
    return <p>Could not load your account.</p>;
  }

  const member = currentMemberQuery.data;
  const mutationStatus =
    updateMutation.error instanceof ApiError
      ? updateMutation.error.status
      : null;

  return (
    <main>
      <Link to="/account">Cancel</Link>
      <h1>Edit account</h1>

      <form onSubmit={handleSubmit}>
        <label>
          Name
          <input
            name="name"
            defaultValue={member.name}
            maxLength={100}
            required
          />
        </label>

        <label>
          Email
          <input
            name="email"
            type="email"
            defaultValue={member.email}
            maxLength={200}
            required
          />
        </label>

        <button type="submit" disabled={updateMutation.isPending}>
          {updateMutation.isPending ? "Saving..." : "Save changes"}
        </button>
      </form>

      {formError && <p>{formError}</p>}
      {mutationStatus === 400 && <p>The API rejected the account data.</p>}
      {mutationStatus === 401 && <p>Your login is missing or expired.</p>}
      {mutationStatus === 403 && (
        <p>You are not allowed to edit this account.</p>
      )}
      {mutationStatus === 404 && <p>Your account no longer exists.</p>}
      {mutationStatus === 409 && (
        <p>This email is already used by another member.</p>
      )}
      {updateMutation.isError && mutationStatus === null && (
        <p>Could not update your account.</p>
      )}
    </main>
  );
}
