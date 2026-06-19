import { ConfigProvider, theme, App as AntApp } from 'antd';
import antdZhTW from 'antd/locale/zh_TW';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { RouterProvider } from 'react-router-dom';
import { router } from './app/router';
import { useUiStore } from './shared/ui/uiStore';
import { brandTokens } from './shared/theme/tokens';

const queryClient = new QueryClient({
  defaultOptions: { queries: { staleTime: 30_000, refetchOnWindowFocus: false } },
});

export default function App() {
  const dark = useUiStore((s) => s.dark);
  return (
    <ConfigProvider
      locale={antdZhTW}
      theme={{ algorithm: dark ? theme.darkAlgorithm : theme.defaultAlgorithm, token: brandTokens }}
    >
      <AntApp>
        <QueryClientProvider client={queryClient}>
          <RouterProvider router={router} />
        </QueryClientProvider>
      </AntApp>
    </ConfigProvider>
  );
}
