import { create } from 'zustand';

export interface AuthState {
  token: string | null;
  companyId: number | null;
  permissions: string[];
  setAuth: (p: Partial<Omit<AuthState, 'setAuth'>>) => void;
}

// 開發階段預設 '*'（全權限）以便瀏覽；接上 iam/Keycloak 後改由登入回傳權限集合填入。
export const useAuthStore = create<AuthState>((set) => ({
  token: null,
  companyId: 1,
  permissions: ['*'],
  setAuth: (p) => set((s) => ({ ...s, ...p })),
}));
