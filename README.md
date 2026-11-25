# Venther.App

Aplicativo mobile **.NET MAUI (.NET 10)** desenvolvido para os técnicos em campo da **Venther**, permitindo o monitoramento em tempo real de equipamentos de refrigeração/climatização (placas IoT ESP32), gestão de chamados (ordens de serviço), visualização de alertas e comissionamento de novos dispositivos.

---

## 1. Visão Geral do Projeto

O **Venther.App** integra-se ao ecossistema Venther composto por:
- **Venther.Api**: Backend REST em ASP.NET Core responsável por autenticação JWT, gestão de clientes, placas, ordens de serviço e histórico de falhas/logs.
- **Placas IoT (ESP32)**: Dispositivos instalados em campo que enviam leituras de temperatura e pressão via **MQTT**.
- **SignalR Hub (`/hubs/telemetry`)**: Canal WebSocket em tempo real que transmite atualizações de telemetria (`TelemetryUpdated`) e falhas (`FaultRaised`) diretamente para o aplicativo.

---

## 2. Tecnologias e Dependências

| Pacote / Tecnologia | Versão | Finalidade |
| :--- | :--- | :--- |
| **.NET MAUI** | `.NET 10` (`net10.0-android`) | Framework multiplataforma focado em Android |
| `CommunityToolkit.Mvvm` | `8.4.2` | Padrão MVVM com source generators (`[ObservableProperty]`, `[RelayCommand]`) |
| `CommunityToolkit.Maui` | `10.0.0` | Comportamentos e controles visuais (ex.: `IconTintColorBehavior`) |
| `Microsoft.AspNetCore.SignalR.Client` | `11.0.0` | Conexão WebSocket em tempo real com `/hubs/telemetry` |
| `Microsoft.Extensions.Http` | `10.0.0` | `HttpClient` tipado (`IApiClient`) com `AuthHeaderHandler` |
| `Microsoft.Extensions.Logging.Debug` | `10.0.0` | Logs de depuração em ambiente `Debug` |

---

## 3. Pré-requisitos

- **.NET 10 SDK** instalado
- Workload **MAUI Android** instalado (`dotnet workload install maui-android`)
- **Android SDK** (API 21+ mínimo) e **ADB** configurados no PATH
- **SQL Server** e **Venther.Api** em execução (localmente ou em rede acessível)

---

## 4. Configuração do Ambiente de Desenvolvimento

### URL da API (`Helpers/ApiConfig.cs`)
Edite `Helpers/ApiConfig.cs` conforme o ambiente:
```csharp
public static class ApiConfig
{
    public const string BaseUrl = "http://localhost:5084";
    public const string ApiPath = "/api";
}
```

### Tráfego HTTP Local (`Platforms/Android/AndroidManifest.xml`)
Para permitir requisições HTTP em desenvolvimento local, `android:usesCleartextTraffic="true"` está habilitado no `AndroidManifest.xml` (remover/desativar em produção com HTTPS).

### Emulador Android vs. Dispositivo Físico (USB)
- **Emulador Android:** Utilize `http://10.0.2.2:5084` em `ApiConfig.BaseUrl` (aponta para o `localhost` da máquina host).
- **Dispositivo Físico via USB:** Mantenha `http://localhost:5084` em `ApiConfig.BaseUrl` e execute o redirecionamento de porta via ADB:
  ```bash
  adb reverse tcp:5084 tcp:5084
  ```

---

## 5. Como Compilar e Executar

### Via CLI (.NET)
```bash
# Restaurar pacotes
dotnet restore Venther.App.csproj

# Compilar para Android (Debug)
dotnet build Venther.App.csproj -f net10.0-android -c Debug

# Compilar para Android (Release)
dotnet build Venther.App.csproj -f net10.0-android -c Release

# Instalar e executar no dispositivo/emulador conectado
dotnet build Venther.App.csproj -t:Run -f net10.0-android
```

### Via Visual Studio
1. Abra `Venther.App.sln` (ou `Venther.App.csproj`) no Visual Studio 2022/2026 com a carga de trabalho **.NET Multi-platform App UI**.
2. Selecione o destino **Android** (emulador ou dispositivo físico via USB).
3. Pressione **F5** para compilar e depurar.

---

## 6. Arquitetura e Estrutura de Pastas

O projeto segue o padrão **MVVM** com injeção de dependência configurada em `MauiProgram.cs` (Services como `Singleton`; ViewModels e Pages como `Transient`):

- **`Views/`**: Páginas XAML organizadas por módulo (`Auth`, `Home`, `Devices`, `Alerts`, `WorkOrders`).
- **`ViewModels/`**: Lógica de apresentação herdando de `BaseViewModel`.
- **`Services/`**: Implementações reais de acesso à API e SignalR (`AuthService`, `DeviceService`, `ClientService`, `WorkOrderService`, `AlertService`, `TelemetryHubService`), além de `Services/Api/` (`ApiClient`, `AuthHeaderHandler`, `SessionService`, `Dtos/`) e `Services/Mocks/` (`MockDeviceProvisioningService`).
- **`Models/`**: Entidades de domínio (`Device`, `WorkOrder`, `AlertItem`, `DeviceTelemetry`).
- **`Controls/`**: Componentes reutilizáveis (`AppHeader`, `AppEntry`, `DeviceCard`, `WorkOrderCard`, `AlertCard`, `MetricCard`, `SegmentedControl`, `StepProgress`).
- **`Helpers/`**: Constantes de rotas (`Routes`), configuração (`ApiConfig`) e validações (`Validators`).
- **`Resources/`**: Tokens de cores (`Colors.xaml`), estilos (`Styles.xaml`), fontes (`Inter`), ícones SVG (`Images/`), ícone do app (`AppIcon/`) e tela de início (`Splash/`).

---

## 7. Funcionalidades, Fluxos e Permissões

- **Autenticação JWT:** Login com e-mail e senha, persistência segura de token/usuário via `SecureStorage` (`SessionService`), restauração automática de sessão ao abrir o app (`AppShell`) e redirecionamento automático para `//login` em respostas HTTP `401`. Em builds `Release`, apenas usuários com perfil `Technician` têm acesso.
- **Recuperação de Senha:** Fluxo completo em 3 etapas (`ForgotPasswordPage` → `CheckEmailPage` com código de 6 dígitos → `ResetPasswordPage` com validação das 5 regras da API → `PasswordResetSuccessPage`).
- **Frota (Home):** Listagem paginada de placas (`RefreshView` + scroll infinito), busca local por nome, cliente ou MAC, filtros por status e atualização dinâmica via SignalR (`Normal`, `Attention`, `Critical`).
- **Cadastro de Dispositivo (Wizard):** Seleção de cliente real (`/customers`), configuração Wi-Fi (simulada) + endereço MAC validado, registro na API (`POST /boards`) e tela de ativação.
- **Telemetria em Tempo Real:** Conexão SignalR em `/hubs/telemetry` com reconexão automática, 4 cards de sensores, tendência térmica (buffer ao vivo de 1h e logs históricos de 12h/24h).
- **Ordens de Serviço e Alertas:** Listagem paginada filtrada por técnico, detalhes com discador (`tel:`) e SMS (`smsto:`), checklist local, conclusão de O.S. (`PUT /maintence_requests/{id}`) e alertas ao vivo via `FaultRaised`.

---

## 8. Endpoints Consumidos e Status da Integração

| Endpoint / Canal | Serviço | Status |
| :--- | :--- | :--- |
| `POST /api/auth/login` | `AuthService` | Integrado |
| `POST /api/auth/logout` | `AuthService` | Integrado |
| `POST /api/auth/forgot-password` | `AuthService` | Integrado |
| `POST /api/auth/reset-password` | `AuthService` | Integrado |
| `GET /api/boards` | `DeviceService` | Integrado |
| `GET /api/boards/{id}` | `DeviceService` | Integrado |
| `POST /api/boards` | `DeviceService` | Integrado |
| `GET /api/boards/{id}/logs` | `DeviceService` | Integrado |
| `GET /api/customers` | `ClientService` | Integrado |
| `GET /api/customers/{id}` | `ClientService` | Integrado |
| `GET /api/maintence_requests` | `WorkOrderService` | Integrado |
| `GET /api/maintence_requests/{id}` | `WorkOrderService` | Integrado |
| `PUT /api/maintence_requests/{id}` | `WorkOrderService` | Integrado |
| `GET /api/failures` | `AlertService` | Integrado |
| `WS /hubs/telemetry` | `TelemetryHubService` | Integrado |
| Provisionamento Wi-Fi ESP32 | `MockDeviceProvisioningService` | Mock (conforme contrato) |
