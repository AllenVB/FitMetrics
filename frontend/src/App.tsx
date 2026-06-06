import { Navigate, Route, Routes } from 'react-router-dom';
import Layout from './components/Layout';
import ProtectedRoute from './components/ProtectedRoute';
import Login from './pages/Login';
import Register from './pages/Register';
import Dashboard from './pages/Dashboard';
import Nutrition from './pages/Nutrition';
import Workouts from './pages/Workouts';
import Progress from './pages/Progress';
import Insights from './pages/Insights';
import AiCoach from './pages/AiCoach';
import AiAssistant from './pages/AiAssistant';
import Knowledge from './pages/Knowledge';
import Dietitian from './pages/Dietitian';
import Profile from './pages/Profile';

export default function App() {
  return (
    <Routes>
      <Route path="/login" element={<Login />} />
      <Route path="/register" element={<Register />} />

      <Route element={<ProtectedRoute />}>
        <Route element={<Layout />}>
          <Route path="/" element={<Dashboard />} />
          <Route path="/nutrition" element={<Nutrition />} />
          <Route path="/workouts" element={<Workouts />} />
          <Route path="/progress" element={<Progress />} />
          <Route path="/insights" element={<Insights />} />
          <Route path="/ai-coach" element={<AiCoach />} />
          <Route path="/ai-assistant" element={<AiAssistant />} />
          <Route path="/knowledge" element={<Knowledge />} />
          <Route path="/dietitian" element={<Dietitian />} />
          <Route path="/profile" element={<Profile />} />
        </Route>
      </Route>

      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  );
}
