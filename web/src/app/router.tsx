import { createBrowserRouter, Navigate } from 'react-router-dom';
import AppLayout from './AppLayout';
import { navLeaves } from './nav';
import DashboardPage from '../features/dashboard/DashboardPage';
import PlaceholderPage from '../features/common/PlaceholderPage';

// 由導覽結構自動推導路由，確保選單與路由不會不同步。
// 已實作頁面用真實元件，其餘先掛 PlaceholderPage（依 docs/04 逐步補上）。
const childRoutes = navLeaves().map((it) => ({
  path: it.key.slice(1),
  element: it.key === '/dashboard' ? <DashboardPage /> : <PlaceholderPage title={it.label} />,
}));

export const router = createBrowserRouter([
  {
    path: '/',
    element: <AppLayout />,
    children: [
      { index: true, element: <Navigate to="/dashboard" replace /> },
      ...childRoutes,
    ],
  },
]);
