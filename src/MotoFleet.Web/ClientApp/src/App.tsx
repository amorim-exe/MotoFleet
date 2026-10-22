import { useEffect, useState } from "react";
import {
  Activity,
  Bike,
  Clock3,
  Fuel,
  Gauge,
  LayoutDashboard,
  LocateFixed,
  MapPin,
  RefreshCw,
  Signal,
  Zap,
} from "lucide-react";
import { Badge } from "./components/ui/badge";
import { Button } from "./components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "./components/ui/card";
import { Progress } from "./components/ui/progress";

type Telemetry = {
  plate: string;
  latitude: number;
  longitude: number;
  speed: number;
  fuelLevelPercentage: number;
  isEngineOn: boolean;
  timestamp: string;
};

const telemetryUrl = "/api/fleet/motos/11111111-2222-3333-4444-555555555555/last-location";
const numberFormat = new Intl.NumberFormat("pt-BR", { maximumFractionDigits: 1 });
const coordinateFormat = new Intl.NumberFormat("pt-BR", { minimumFractionDigits: 5, maximumFractionDigits: 5 });

function normalizeTelemetry(payload: unknown): Telemetry {
  const values = payload as Record<string, unknown>;

  return {
    plate: String(values.plate ?? values.Plate ?? ""),
    latitude: Number(values.latitude ?? values.Latitude),
    longitude: Number(values.longitude ?? values.Longitude),
    speed: Number(values.speed ?? values.Speed),
    fuelLevelPercentage: Number(values.fuelLevelPercentage ?? values.FuelLevelPercentage),
    isEngineOn: Boolean(values.isEngineOn ?? values.IsEngineOn ?? false),
    timestamp: String(values.timestamp ?? values.Timestamp ?? ""),
  };
}

function App() {
  const [telemetry, setTelemetry] = useState<Telemetry | null>(null);
  const [lastChecked, setLastChecked] = useState<Date | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(false);

  useEffect(() => {
    let active = true;
    let controller: AbortController | undefined;

    const refresh = async () => {
      controller?.abort();
      controller = new AbortController();
      try {
        const response = await fetch(telemetryUrl, { signal: controller.signal });
        if (!response.ok) throw new Error("Telemetry unavailable");
        const result = normalizeTelemetry(await response.json());
        if (active) {
          setTelemetry(result);
          setError(false);
        }
      } catch (cause) {
        if (active && cause instanceof Error && cause.name !== "AbortError") {
          setError(true);
        }
      } finally {
        if (active) {
          setLoading(false);
          setLastChecked(new Date());
        }
      }
    };

    void refresh();
    const interval = window.setInterval(() => void refresh(), 5000);
    return () => {
      active = false;
      controller?.abort();
      window.clearInterval(interval);
    };
  }, []);

  const online = telemetry !== null && !error;
  const timestamp = telemetry?.timestamp ? new Date(telemetry.timestamp) : null;

  return (
    <div className="app-shell">
      <aside className="sidebar">
        <a className="brand" href="/" aria-label="MotoFleet, início">
          <span className="brand-mark"><Bike size={21} strokeWidth={2.2} /></span>
          <span className="brand-name">moto<span>fleet</span></span>
        </a>

        <div className="sidebar-label">GESTÃO</div>
        <nav aria-label="Navegação principal">
          <a className="nav-item active" href="/" aria-current="page">
            <LayoutDashboard size={18} />
            <span>Visão geral</span>
          </a>
          <a className="nav-item disabled" href="#frota" aria-disabled="true" onClick={(event) => event.preventDefault()}>
            <Bike size={18} />
            <span>Frota</span>
            <span className="nav-count">01</span>
          </a>
        </nav>

        <div className="sidebar-bottom">
          <div className="connection-indicator"><span /> API conectada</div>
          <div className="sidebar-version">MOTOFLEET OPERATIONS <span>v1.0</span></div>
        </div>
      </aside>

      <main className="main-content">
        <header className="topbar">
          <div className="breadcrumbs"><span>Gestão</span><span className="crumb-divider">/</span><strong>Visão geral</strong></div>
          <div className="topbar-meta"><span className="live-mark"><span /> AO VIVO</span><span className="topbar-date">Telemetria em tempo real</span></div>
        </header>

        <div className="dashboard-content">
          <section className="page-heading">
            <div>
              <div className="eyebrow">CENTRAL DE OPERAÇÕES</div>
              <h1>Visão geral</h1>
              <p>Acompanhe o estado e a localização da sua frota.</p>
            </div>
            <Button variant="outline" size="sm" onClick={() => window.location.reload()} aria-label="Atualizar painel">
              <RefreshCw size={15} /> Atualizar
            </Button>
          </section>

          <section className="fleet-summary" aria-label="Resumo da frota">
            <div className="summary-item">
              <span className="summary-label">VEÍCULOS MONITORADOS</span>
              <span className="summary-value">01</span>
            </div>
            <div className="summary-divider" />
            <div className="summary-item">
              <span className="summary-label">EM OPERAÇÃO</span>
              <span className="summary-value with-status"><i className={online ? "status-dot online" : "status-dot"} />{online ? "01" : "00"}</span>
            </div>
            <div className="summary-divider" />
            <div className="summary-item">
              <span className="summary-label">ÚLTIMA VERIFICAÇÃO</span>
              <span className="summary-time">{lastChecked ? lastChecked.toLocaleTimeString("pt-BR", { hour: "2-digit", minute: "2-digit", second: "2-digit" }) : "--:--:--"}</span>
            </div>
          </section>

          <section className="vehicle-heading">
            <div className="vehicle-title">
              <span className="vehicle-icon"><Bike size={20} /></span>
              <div><h2>{telemetry?.plate || "Moto monitorada"}</h2><span>Unidade de telemetria</span></div>
            </div>
            <Badge variant={online ? "success" : "warning"}>
              <span className="badge-dot" />{online ? "Online" : loading ? "Conectando" : "Sem sinal"}
            </Badge>
          </section>

          {error && !telemetry && (
            <div className="notice" role="status">
              <Signal size={17} />
              <div><strong>Aguardando telemetria</strong><span>Os dados serão exibidos assim que a API receber uma atualização da moto.</span></div>
            </div>
          )}

          <section className="metric-grid" aria-label="Indicadores do veículo">
            <Card className="metric-card speed-card">
              <CardHeader className="metric-header"><CardTitle>Velocidade atual</CardTitle><span className="metric-icon blue"><Gauge size={18} /></span></CardHeader>
              <CardContent>
                <div className="metric-number">{telemetry ? numberFormat.format(telemetry.speed) : "--"}<span>km/h</span></div>
                <div className="metric-foot"><Activity size={14} /> Leitura instantânea</div>
              </CardContent>
            </Card>

            <Card className="metric-card fuel-card">
              <CardHeader className="metric-header"><CardTitle>Nível de combustível</CardTitle><span className="metric-icon amber"><Fuel size={18} /></span></CardHeader>
              <CardContent>
                <div className="metric-number">{telemetry ? `${telemetry.fuelLevelPercentage}` : "--"}<span>%</span></div>
                <Progress className="fuel-progress" value={telemetry?.fuelLevelPercentage ?? 0} aria-label="Nível de combustível" />
                <div className="metric-foot">{telemetry ? telemetry.fuelLevelPercentage >= 25 ? "Nível adequado" : "Nível baixo" : "Aguardando leitura"}</div>
              </CardContent>
            </Card>

            <Card className="metric-card engine-card">
              <CardHeader className="metric-header"><CardTitle>Motor</CardTitle><span className="metric-icon green"><Zap size={18} /></span></CardHeader>
              <CardContent>
                <div className="metric-number engine-value">{telemetry ? telemetry.isEngineOn ? "Ligado" : "Desligado" : "--"}</div>
                <div className="metric-foot"><i className={telemetry?.isEngineOn ? "status-dot online" : "status-dot"} /> Estado de ignição</div>
              </CardContent>
            </Card>
          </section>

          <section className="details-grid">
            <Card className="location-card">
              <CardHeader className="section-card-header">
                <div><div className="section-kicker">LOCALIZAÇÃO</div><CardTitle>Posição do veículo</CardTitle></div>
                <span className="metric-icon green"><MapPin size={18} /></span>
              </CardHeader>
              <CardContent>
                <div className="coordinates">
                  <div className="coordinate-row"><span>Latitude</span><code>{telemetry ? coordinateFormat.format(telemetry.latitude) : "--"}</code></div>
                  <div className="coordinate-row"><span>Longitude</span><code>{telemetry ? coordinateFormat.format(telemetry.longitude) : "--"}</code></div>
                </div>
                {telemetry && <a className="map-link" href={`https://maps.google.com/?q=${telemetry.latitude},${telemetry.longitude}`} target="_blank" rel="noreferrer"><LocateFixed size={15} /> Abrir localização no mapa</a>}
                {!telemetry && <div className="location-empty"><MapPin size={18} /> Aguardando coordenadas GPS</div>}
              </CardContent>
            </Card>

            <Card className="update-card">
              <CardHeader className="section-card-header">
                <div><div className="section-kicker">ATIVIDADE</div><CardTitle>Última atualização</CardTitle></div>
                <span className="metric-icon blue"><Clock3 size={18} /></span>
              </CardHeader>
              <CardContent>
                <div className="update-time">{timestamp ? timestamp.toLocaleTimeString("pt-BR", { hour: "2-digit", minute: "2-digit" }) : "--:--"}<span>{timestamp ? timestamp.toLocaleDateString("pt-BR", { day: "2-digit", month: "short", year: "numeric" }) : "Sem dados recebidos"}</span></div>
                <div className="update-rule"><span /></div>
                <div className="update-foot"><span className="live-mark"><span /> MONITORAMENTO</span><span>Intervalo de 5 segundos</span></div>
              </CardContent>
            </Card>
          </section>

          <footer className="page-footer"><span>MotoFleet <span className="footer-separator">/</span> Monitoramento de frota</span><span>{error ? "A API não respondeu" : "Sistema operacional"}</span></footer>
        </div>
      </main>
    </div>
  );
}

export default App;