import { useEffect } from "react";
import { useQuery } from "@tanstack/react-query";
import { Navigate } from "react-router-dom";
import { ApiError } from "../api";
import { getCurrentMember } from "./authApi";
import { getAccessToken, removeAccessToken } from "./tokenStorage";

export function AccountPage() {
  const currentMemberQuery = useQuery({
    // lees (GET)
    queryKey: ["current-member"], // zet de data in deze key
    queryFn: getCurrentMember,
    enabled: getAccessToken() !== null,
    retry: false, // als ik niet (retry) zet, en api back (401 Unauthorized) DUS >> react query gaat call (getCurrentMember) function again..
  });

  // currentMemberQuery.error instanceof ApiError >> result is true of false
  const unauthorized =
    currentMemberQuery.error instanceof ApiError &&
    currentMemberQuery.error.status === 401;

  // This effect removes the saved access token when the API returns 401 Unauthorized. This forces the user to log in again with a valid token.
  useEffect(() => {
    if (unauthorized) {
      removeAccessToken();
    }
  }, [unauthorized]);

  if (!getAccessToken()) {
    return <Navigate to="/login" replace />;
  }

  if (currentMemberQuery.isPending) {
    return <p>Loading account...</p>;
  }

  if (unauthorized) {
    return <Navigate to="/login" replace />;
  }

  if (currentMemberQuery.isError) {
    return <p>Could not load the account.</p>;
  }

  const member = currentMemberQuery.data;

  return (
    <main>
      <h1>{member.name}</h1>
      <p>{member.email}</p>
      <p>Role: {member.role}</p>
    </main>
  );
}
