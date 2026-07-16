import { useEffect } from "react";
import { useQuery } from "@tanstack/react-query";
import { ApiError } from "../api";
import { getCurrentMember } from "./authApi";
import { getAccessToken, removeAccessToken } from "./tokenStorage";

export function useCurrentMember() {
  const query = useQuery({
    queryKey: ["current-member"], // zet de data in deze key
    queryFn: getCurrentMember,
    enabled: getAccessToken() !== null,
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
