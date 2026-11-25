# Status do Projeto — Venther.App

## 1. Resumo
Integração completa do aplicativo mobile .NET MAUI (.NET 10, foco Android) com a API Venther e SignalR (`/hubs/telemetry`). Todas as 7 etapas concluídas: infraestrutura HTTP/Sessão, autenticação, frota/cadastro, ordens de serviço, telemetria em tempo real, alertas e limpeza final.

## 2. Plano de integração
- [x] **Etapa 1:** Infraestrutura de API (`ApiConfig`, `ApiException`, `IApiClient`/`ApiClient`, `AuthHeaderHandler`, `ISessionService`/`SessionService`, DTOs)
- [x] **Etapa 2:** Autenticação real (`AuthService`, `LoginViewModel`, `AppShell`, `Validators`, fluxo de senha e logout no `AppHeader`)
- [x] **Etapa 3:** Frota e cadastro (`DeviceService`, `ClientService`, `HomeViewModel` com paginação/RefreshView, wizard de cadastro com `POST /boards`)
- [x] **Etapa 4:** Ordens de serviço (`WorkOrderService`, `WorkOrdersViewModel` com paginação/RefreshView, `WorkOrderDetailViewModel` com discador/SMS e `PUT /maintence_requests/{id}`)
- [x] **Etapa 5:** Telemetria em tempo real (`TelemetryHubService` SignalR, `DeviceDetailViewModel`, `HomeViewModel`, `WorkOrderDetailViewModel`, `DeviceActivatedViewModel`)
- [x] **Etapa 6:** Alertas / Falhas (`AlertService`, `AlertsViewModel` com `GET /failures` + `FaultRaised`, paginação e fallback 401/403)
- [x] **Etapa 7:** Limpeza final, builds Debug/Release e checklist de testes manuais

## 3. Telas
| Tela | Dados | Status | Observações |
| :--- | :--- | :--- | :--- |
| `LoginPage` | API | Concluída | Login real; bloqueia não-técnicos em Release |
| `ForgotPasswordPage` | API | Concluída | Envia código de 6 dígitos |
| `CheckEmailPage` | API | Concluída | Código de 6 dígitos e reenvio |
| `ResetPasswordPage` | API | Concluída | 5 regras da API + força |
| `PasswordResetSuccessPage` | Estática | Concluída | Redireciona para `//login` |
| `HomePage` | API + SignalR | Concluída | `RefreshView`, paginação, busca, status ao vivo |
| `RegisterDevicePage` | API | Concluída | `/customers` (pageSize 100), 3 chips |
| `DeviceConnectionPage` | Parcial | Concluída | Wi-Fi mock + MAC + `POST /boards` |
| `DeviceActivatedPage` | API + SignalR | Concluída | Dados reais; telemetria se disponível |
| `DeviceDetailPage` | API + SignalR | Concluída | 4 cards, tendência 1h/12h/24h |
| `WorkOrdersPage` | API | Concluída | `RefreshView`, paginação, abas |
| `WorkOrderDetailPage` | API + SignalR | Concluída | OS/placa/cliente, discador/SMS, conclusão |
| `AlertsPage` | API + SignalR | Concluída | `RefreshView`, paginação, `FaultRaised`, fallback 401/403 |

## 4. Endpoints consumidos
| Endpoint | Usado por | Status |
| :--- | :--- | :--- |
| `POST /auth/login` | `AuthService.LoginAsync` | Integrado |
| `POST /auth/logout` | `AuthService.LogoutAsync` | Integrado |
| `POST /auth/forgot-password` | `AuthService.ForgotPasswordAsync` | Integrado |
| `POST /auth/reset-password` | `AuthService.ResetPasswordAsync` | Integrado |
| `GET /boards` | `DeviceService.GetDevicesAsync` | Integrado |
| `GET /boards/{id}` | `DeviceService.GetDeviceByIdAsync` | Integrado |
| `POST /boards` | `DeviceService.RegisterDeviceAsync` | Integrado |
| `GET /boards/{id}/logs` | `DeviceService.GetBoardLogsAsync` | Integrado |
| `GET /customers` | `ClientService.GetCustomersAsync` | Integrado |
| `GET /customers/{id}` | `ClientService.GetCustomerByIdAsync` | Integrado |
| `GET /maintence_requests` | `WorkOrderService.GetWorkOrdersAsync` | Integrado |
| `GET /maintence_requests/{id}` | `WorkOrderService.GetWorkOrderByIdAsync` | Integrado |
| `PUT /maintence_requests/{id}` | `WorkOrderService.CompleteWorkOrderAsync` | Integrado |
| `GET /failures` | `AlertService.GetAlertsAsync` | Integrado |
| `WS /hubs/telemetry` | `TelemetryHubService` | Integrado |

## 5. Mocks e hardcodes restantes
- `Services/Mocks/MockDeviceProvisioningService.cs`: provisionamento Wi-Fi ESP32 simulado (mantido conforme contrato).
- `Helpers/ApiConfig.cs`: `BaseUrl = "http://localhost:5084"` (desenvolvimento; alterar para produção).
- `ViewModels/DeviceConnectionViewModel.cs:31-37`: SSID/canal e MAC inicial padrão para o mock de provisionamento.

## 6. Decisões e desvios (Figma/API)
- `AuthHeaderHandler`: qualquer HTTP 401 com token ativo (exceto `/auth/login`) limpa a sessão e navega para `//login` na thread principal.
- `TelemetryHubService`: `WithAutomaticReconnect()` + try/catch não travam o app se o servidor/broker estiver offline.
- `AlertsViewModel`: TODO mantido para badge na TabBar do Shell.

## 7. Bugs e pendências
- 2 avisos CS0618 pré-existentes em `Controls/WorkOrderCard.xaml.cs:97-98` (`ScaleTo` no build Release).

## 8. Resultado do build
- **Debug:** `dotnet build -r android-arm64 -c Debug` — **Sucesso** (0 erros, 0 avisos).
- **Release:** `dotnet build -r android-arm64 -c Release` — **Sucesso** (0 erros, 2 avisos CS0618 em `WorkOrderCard.xaml.cs`).

## 9. Próximo passo (Checklist de Testes Manuais)
- [ ] **Login / Sessão:** Login válido, erro 400 (`"E-mail ou senha inválidos."`), restauração ao reabrir app, logout no avatar.
- [ ] **Recuperação de Senha:** `ForgotPassword` -> `CheckEmail` (código 6 dígitos + reenviar) -> `ResetPassword` (5 regras) -> `PasswordResetSuccess`.
- [ ] **Home:** Pull-to-refresh, busca por nome/cliente/MAC, filtros Total/Normais/Atenção, paginação ao rolar.
- [ ] **Cadastro (Wizard):** Seleção de cliente real (`/customers`), validação de MAC, erro 409/400, ativação.
- [ ] **DeviceDetail:** Assinatura SignalR ao vivo, troca de abas e períodos (1h/12h/24h).
- [ ] **Chamados:** Listagem filtrada por técnico, detalhes com discador/SMS, conclusão com confirmação.
- [ ] **Alertas:** Listagem `/failures` (ou fallback 401/403), recebimento de `FaultRaised` ao vivo, navegação `"Ver"`.
