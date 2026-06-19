import type { ReactNode } from 'react';
import { usePermission } from './usePermission';

// 權限驅動顯示：無權限則不渲染（按鈕/區塊層級控管）。
export function Can({ permission, children }: { permission?: string; children: ReactNode }) {
  const { can } = usePermission();
  return can(permission) ? <>{children}</> : null;
}
