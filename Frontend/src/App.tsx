import { Link, Route, Routes } from "react-router-dom";
import { AccountPage } from "./auth/AccountPage";
import { LoginPage } from "./auth/LoginPage";
import { LogoutButton } from "./auth/LogoutButton";
import { RequireAdministrator } from "./auth/RequireAdministrator";
import { BookListPage } from "./books/BookListPage";
import { BookDetailsPage } from "./books/BookDetailsPage";
import { CreateBookPage } from "./books/CreateBookPage";
import { EditBookPage } from "./books/EditBookPage";

function HomePage() {
  return <h1>Book Tracker</h1>;
}

export default function App() {
  return (
    <>
      <nav>
        <Link to="/">Home</Link>
        <Link to="/books">Books</Link>
        <Link to="/login">Log in</Link>
        <Link to="/account">Account</Link>
        <LogoutButton />
      </nav>

      <Routes>
        <Route path="/" element={<HomePage />} />
        <Route path="/books" element={<BookListPage />} />

        <Route element={<RequireAdministrator />}>
          <Route path="/books/new" element={<CreateBookPage />} />
          <Route path="/books/:bookId/edit" element={<EditBookPage />} />
        </Route>

        <Route path="/books/:bookId" element={<BookDetailsPage />} />
        <Route path="/login" element={<LoginPage />} />
        <Route path="/account" element={<AccountPage />} />
      </Routes>
    </>
  );
}
