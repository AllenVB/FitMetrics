import { Outlet } from 'react-router-dom';

// Test modunda auth devre dışı — direkt içeriğe geç
export default function ProtectedRoute() {
  return <Outlet />;
}
