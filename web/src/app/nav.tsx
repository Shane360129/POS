import type { ReactNode } from 'react';
import {
  DashboardOutlined,
  AppstoreOutlined,
  ShoppingCartOutlined,
  ShopOutlined,
  InboxOutlined,
  AccountBookOutlined,
  SettingOutlined,
} from '@ant-design/icons';

export interface NavItem {
  key: string; // 完整路徑，如 /mdm/products
  label: string;
  icon?: ReactNode;
  permission?: string;
  children?: NavItem[];
}

// 資訊架構：依三大流程（O2C/P2P/Plan-to-Produce）與主檔/財務組織（docs/04 §資訊架構）。
export const navGroups: NavItem[] = [
  { key: '/dashboard', label: '儀表板', icon: <DashboardOutlined />, permission: 'dashboard:read' },
  {
    key: '/mdm', label: '主檔', icon: <AppstoreOutlined />,
    children: [
      { key: '/mdm/products', label: '商品 / 料件', permission: 'product:read' },
      { key: '/mdm/customers', label: '客戶', permission: 'customer:read' },
      { key: '/mdm/suppliers', label: '供應商', permission: 'supplier:read' },
      { key: '/mdm/warehouses', label: '倉庫', permission: 'warehouse:read' },
    ],
  },
  {
    key: '/procure', label: '採購 (P2P)', icon: <ShoppingCartOutlined />,
    children: [
      { key: '/procure/requisitions', label: '請購', permission: 'requisition:read' },
      { key: '/procure/orders', label: '採購單', permission: 'po:read' },
      { key: '/procure/receipts', label: '進貨驗收', permission: 'receipt:read' },
    ],
  },
  {
    key: '/sales', label: '銷售 (O2C)', icon: <ShopOutlined />,
    children: [
      { key: '/sales/quotations', label: '報價', permission: 'quotation:read' },
      { key: '/sales/orders', label: '訂單', permission: 'so:read' },
      { key: '/sales/shipments', label: '出貨', permission: 'shipment:read' },
      { key: '/sales/returns', label: '退貨 / 折讓', permission: 'return:read' },
    ],
  },
  {
    key: '/inv', label: '庫存', icon: <InboxOutlined />,
    children: [
      { key: '/inv/balances', label: '庫存查詢（多倉）', permission: 'stock:read' },
      { key: '/inv/movements', label: '庫存異動', permission: 'movement:read' },
      { key: '/inv/counts', label: '盤點', permission: 'count:read' },
    ],
  },
  {
    key: '/fin', label: '財務會計', icon: <AccountBookOutlined />,
    children: [
      { key: '/fin/journal', label: '總帳分錄', permission: 'journal:read' },
      { key: '/fin/ar', label: '應收帳款', permission: 'ar:read' },
      { key: '/fin/ap', label: '應付帳款', permission: 'ap:read' },
      { key: '/fin/invoices', label: '統一發票', permission: 'invoice:read' },
      { key: '/fin/trial-balance', label: '試算表', permission: 'report:read' },
      { key: '/fin/periods', label: '會計期間', permission: 'period:read' },
    ],
  },
  {
    key: '/system', label: '系統', icon: <SettingOutlined />,
    children: [
      { key: '/iam/users', label: '使用者 / 角色', permission: 'iam:read' },
      { key: '/audit/logs', label: '稽核軌跡', permission: 'audit:read' },
    ],
  },
];

export function navLeaves(items: NavItem[] = navGroups): NavItem[] {
  return items.flatMap((it) => (it.children ? navLeaves(it.children) : [it]));
}
