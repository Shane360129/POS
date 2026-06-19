import { Card, Empty, Typography } from 'antd';
import { useTranslation } from 'react-i18next';

export default function PlaceholderPage({ title }: { title: string }) {
  const { t } = useTranslation();
  return (
    <Card>
      <Typography.Title level={4} style={{ marginTop: 0 }}>{title}</Typography.Title>
      <Empty description={`「${title}」${t('common.building')}`} />
    </Card>
  );
}
