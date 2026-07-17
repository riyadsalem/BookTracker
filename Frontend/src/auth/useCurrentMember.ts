import { useEffect } from "react";
import { useQuery } from "@tanstack/react-query";
import { ApiError } from "../api";
import { getCurrentMember } from "./authApi";
import { removeAccessToken, useAccessToken } from "./tokenStorage";

export function useCurrentMember() {
  // useAccessToken keeps this hook in sync with the current access token.
  // If the user logs in, logs out, or the token is removed,
  // React automatically re-renders and updates the query.
  const accessToken = useAccessToken();

  const query = useQuery({
    queryKey: ["current-member"], // zet de data in deze key
    queryFn: getCurrentMember,
    enabled: accessToken !== null,
    retry: false, // als ik niet (retry) zet, en api back (401 Unauthorized) DUS >> react query gaat call (getCurrentMember) function again..
  });

  // currentMemberQuery.error instanceof ApiError >> result is true of false
  const unauthorized =
    query.error instanceof ApiError && query.error.status === 401;

  // This effect removes the saved access token when the API returns 401 Unauthorized. This forces the user to log in again with a valid token.
  useEffect(() => {
    if (unauthorized) {
      removeAccessToken();
    }
  }, [unauthorized]);

  return query; // Id, Name, Email, Role
}
