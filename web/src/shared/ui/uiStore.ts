import { create } from 'zustand';
import { persist } from 'zustand/middleware';

interface UiState {
  dark: boolean;
  toggleDark: () => void;
}

// 少量 UI 狀態交給 Zustand（伺服器狀態一律走 TanStack Query）。
export const useUiStore = create<UiState>()(
  persist(
    (set) => ({
      dark: false,
      toggleDark: () => set((s) => ({ dark: !s.dark })),
    }),
    { name: 'invenflow-ui' },
  ),
);
