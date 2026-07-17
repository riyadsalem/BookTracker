export type RegisterMemberRequest = {
  name: string;
  email: string;
  password: string;
};

export type RegisterMemberResponse = {
  id: number;
  name: string;
  email: string;
};

export type MemberSummary = {
  id: number;
  name: string;
  email: string;
};

export type MemberDetails = {
  id: number;
  name: string;
  email: string;
};

export type GetMembersRequest = {
  page: number;
  pageSize: number;
  search: string;
};

// Only name and email - no version, password, or role.
export type UpdateMemberRequest = {
  name: string;
  email: string;
};
