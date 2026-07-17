import { Route, Routes } from "react-router-dom";
import { AccountPage } from "./auth/AccountPage";
import { EditAccountPage } from "./auth/EditAccountPage";
import { LoginPage } from "./auth/LoginPage";
import { RequireAdministrator } from "./auth/RequireAdministrator";
import { RequireAuthenticated } from "./auth/RequireAuthenticated";
import { BookListPage } from "./books/BookListPage";
import { BookDetailsPage } from "./books/BookDetailsPage";
import { CreateBookPage } from "./books/CreateBookPage";
import { EditBookPage } from "./books/EditBookPage";
import { RegisterPage } from "./members/RegisterPage";
import { MemberListPage } from "./members/MemberListPage";
import { MemberDetailsPage } from "./members/MemberDetailsPage";
import { EditMemberPage } from "./members/EditMemberPage";
import { Navigation } from "./Navigation";

function HomePage() {
  return <h1>Book Tracker</h1>;
}

export default function App() {
  return (
    <>
      <Navigation />

      <Routes>
        <Route path="/" element={<HomePage />} />
        <Route path="/books" element={<BookListPage />} />

        <Route element={<RequireAdministrator />}>
          <Route path="/books/new" element={<CreateBookPage />} />
          <Route path="/books/:bookId/edit" element={<EditBookPage />} />
          <Route path="/members" element={<MemberListPage />} />
          <Route path="/members/:memberId" element={<MemberDetailsPage />} />
          <Route path="/members/:memberId/edit" element={<EditMemberPage />} />
        </Route>

        <Route path="/books/:bookId" element={<BookDetailsPage />} />

        <Route path="/register" element={<RegisterPage />} />
        <Route path="/login" element={<LoginPage />} />

        <Route element={<RequireAuthenticated />}>
          <Route path="/account" element={<AccountPage />} />
          <Route path="/account/edit" element={<EditAccountPage />} />
        </Route>
      </Routes>
    </>
  );
}
