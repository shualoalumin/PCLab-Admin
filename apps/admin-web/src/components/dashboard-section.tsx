type DashboardSectionProps = {
  title: string;
  hint: string;
  children: React.ReactNode;
};

export function DashboardSection({
  title,
  hint,
  children,
}: DashboardSectionProps) {
  return (
    <section className="card section-card">
      <header>
        <div className="title-stack">
          <h2>{title}</h2>
          <p className="muted">{hint}</p>
        </div>
      </header>

      {children}
    </section>
  );
}
