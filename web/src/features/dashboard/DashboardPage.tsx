import { Card, Col, Row, Statistic } from 'antd';
import ReactECharts from 'echarts-for-react';
import type { EChartsOption } from 'echarts';

export default function DashboardPage() {
  // 示範資料；接上後端後改由 TanStack Query 取 /api/reports/dashboard。
  const salesTrend: EChartsOption = {
    tooltip: { trigger: 'axis' },
    grid: { left: 48, right: 16, top: 24, bottom: 32 },
    xAxis: { type: 'category', data: ['06-13', '06-14', '06-15', '06-16', '06-17', '06-18', '06-19'] },
    yAxis: { type: 'value' },
    series: [{ name: '銷售額', type: 'line', smooth: true, areaStyle: {}, data: [82000, 93200, 90100, 93400, 129000, 133000, 132000] }],
  };

  return (
    <div>
      <Row gutter={[16, 16]}>
        <Col xs={12} md={6}><Card><Statistic title="本月營收" value={1280000} prefix="NT$" /></Card></Col>
        <Col xs={12} md={6}><Card><Statistic title="毛利率" value={32.5} suffix="%" valueStyle={{ color: '#16A34A' }} /></Card></Col>
        <Col xs={12} md={6}><Card><Statistic title="存貨價值" value={860000} prefix="NT$" /></Card></Col>
        <Col xs={12} md={6}><Card><Statistic title="低於安全庫存" value={4} suffix="項" valueStyle={{ color: '#D97706' }} /></Card></Col>
      </Row>
      <Card title="近 7 日銷售趨勢" style={{ marginTop: 16 }}>
        <ReactECharts option={salesTrend} style={{ height: 320 }} />
      </Card>
    </div>
  );
}
