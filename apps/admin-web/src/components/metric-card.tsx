type MetricCardProps = {
  label: string;
  value: string;
  hint: string;
};

export function MetricCard({ label, value, hint }: MetricCardProps) {
  return (
    <article className="card metric-card">
      <h2>{label}</h2>
      <div className="metric-value">{value}</div>
      <p className="muted">{hint}</p>
    </article>
  );
}
