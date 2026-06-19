import { useMemo } from 'react';
import { Layout, Menu, Space, Switch, Typography } from 'antd';
import type { MenuProps } from 'antd';
import { BulbOutlined } from '@ant-design/icons';
import { Outlet, useLocation, useNavigate } from 'react-router-dom';
import { navGroups } from './nav';
import type { NavItem } from './nav';
import { usePermission } from '../shared/auth/usePermission';
import { useUiStore } from '../shared/ui/uiStore';

const { Sider, Header, Content } = Layout;

function toMenuItems(items: NavItem[], can: (p?: string) => boolean): MenuProps['items'] {
  return items
    .filter((it) => can(it.permission))
    .map((it) => ({
      key: it.key,
      icon: it.icon,
      label: it.label,
      children: it.children ? toMenuItems(it.children, can) : undefined,
    }));
}

export default function AppLayout() {
  const navigate = useNavigate();
  const location = useLocation();
  const { can } = usePermission();
  const dark = useUiStore((s) => s.dark);
  const toggleDark = useUiStore((s) => s.toggleDark);

  // 權限驅動選單：無權限的項目不渲染（真正權限仍由後端 enforce）。
  const items = useMemo(() => toMenuItems(navGroups, can), [can]);

  return (
    <Layout style={{ minHeight: '100vh' }}>
      <Sider theme={dark ? 'dark' : 'light'} width={224} breakpoint="lg" collapsible>
        <div style={{ height: 56, display: 'flex', alignItems: 'center', justifyContent: 'center', fontWeight: 700, fontSize: 16 }}>
          InvenFlow ERP
        </div>
        <Menu
          mode="inline"
          theme={dark ? 'dark' : 'light'}
          selectedKeys={[location.pathname]}
          defaultOpenKeys={navGroups.map((g) => g.key)}
          items={items}
          onClick={(e) => navigate(e.key)}
        />
      </Sider>
      <Layout>
        <Header style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', paddingInline: 16 }}>
          <Typography.Text strong>企業資源規劃 MIS</Typography.Text>
          <Space>
            <BulbOutlined />
            <Switch checked={dark} onChange={toggleDark} checkedChildren="深" unCheckedChildren="淺" />
          </Space>
        </Header>
        <Content style={{ margin: 16 }}>
          <Outlet />
        </Content>
      </Layout>
    </Layout>
  );
}
