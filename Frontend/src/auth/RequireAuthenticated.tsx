import { Navigate, Outlet } from "react-router-dom";
import { getAccessToken } from "./tokenStorage";
import { useCurrentMember } from "./useCurrentMember";

export function RequireAuthenticated() {
  const currentMemberQuery = useCurrentMember();

  if (!getAccessToken()) {
    return <Navigate to="/login" replace />;
  }

  if (currentMemberQuery.isPending) {
    return <p>Checking your account...</p>;
  }

  if (currentMemberQuery.isError) {
    return <Navigate to="/login" replace />;
  }

  return <Outlet />;
}
