import { useAuthStore } from './authStore';

// 權限判斷 hook。原則：前端遮罩只是體驗，真正權限一律由後端 enforce（docs/04 §權限驅動 UI）。
export function usePermission() {
  const permissions = useAuthStore((s) => s.permissions);
  const can = (perm?: string): boolean => {
    if (!perm) return true;
    if (permissions.includes('*')) return true;
    return permissions.includes(perm);
  };
  return { can };
}
