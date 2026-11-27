# Status do Projeto — Venther.App

## 1. Resumo
Aplicativo mobile .NET MAUI (.NET 10, Android) para técnicos da Venther integrado com API REST e SignalR. Diagnóstico e correção de conectividade com a API local em `5084`: configuração de `network_security_config.xml` (tráfego cleartext HTTP permitido para desenvolvimento), saneamento de URL no `ApiClient` e `ApiConfig` contra duplicação de `/api`, atualização da URL padrão para `5084` e binding da API em `0.0.0.0:5084`.

## 2. Plano de integração
- [x] **Etapa 1:** Infraestrutura de API (`ApiConfig`, `ApiException`, `IApiClient`/`ApiClient`, `AuthHeaderHandler`, `ISessionService`/`SessionService`, DTOs)
- [x] **Etapa 2:** Autenticação real (`AuthService`, `LoginViewModel`, `AppShell`, `Validators`, fluxo de senha e logout)
- [x] **Etapa 3:** Frota e cadastro (`DeviceService`, `ClientService`, `HomeViewModel` com paginação/RefreshView, wizard de cadastro com `POST /boards`)
- [x] **Etapa 4:** Ordens de serviço (`WorkOrderService`, `WorkOrdersViewModel` com paginação/RefreshView, `WorkOrderDetailViewModel` com discador/SMS e `PUT /maintence_requests/{id}`)
- [x] **Etapa 5:** Telemetria em tempo real (`TelemetryHubService` SignalR, `DeviceDetailViewModel`, `HomeViewModel`, `WorkOrderDetailViewModel`, `DeviceActivatedViewModel`)
- [x] **Etapa 6:** Alertas / Falhas (`AlertService`, `AlertsViewModel` com `GET /failures` + `FaultRaised`, paginação e fallback 401/403)
- [x] **Etapa 7:** Suporte offline básico (`ICacheService`, `ISyncService`, fila de checkout, faixa discreta nas telas e limpeza no logout)
- [x] **Padronização de Header e Conectividade:** `AppHeader` universal, `network_security_config.xml` e saneamento dinâmico de endpoints.

## 3. Telas
| Tela | Dados | Status | Observações |
| :--- | :--- | :--- | :--- |
| `LoginPage` | API | Concluída | Long-press no logo em `DEBUG` abre `PromptPopup` customizado |
| `ForgotPasswordPage` | API | Concluída | Envia código de 6 dígitos por e-mail |
| `CheckEmailPage` | API | Concluída | `AlertPopup` de sucesso ao reenviar código |
| `ResetPasswordPage` | API | Concluída | 5 regras da API + força da senha |
| `PasswordResetSuccessPage` | Estática | Concluída | Redireciona para `//login` |
| `HomePage` | API + Cache + SignalR | Concluída | `AppHeader` padrão (Branding Venther + Alertas + Perfil) |
| `RegisterDevicePage` | API + Cache | Concluída | `AppHeader` (`ShowBackButton`, `Subtitle` e `StepProgress` no `BottomContent`) |
| `DeviceConnectionPage` | Parcial | Concluída | `AppHeader` (`ShowBackButton`, `Subtitle`, `StepProgress` e Hardware Pill no `BottomContent`) |
| `DeviceActivatedPage` | API + SignalR | Concluída | `AppHeader` (`ShowBackButton`, mais opções no `RightActionContent` e progresso 3/3) |
| `DeviceDetailPage` | API + Cache + SignalR | Concluída | `AppHeader` (`ShowBackButton`, título dinâmico e perfil) |
| `WorkOrdersPage` | API + Cache | Concluída | `AppHeader` (`SegmentedControl` integrado no `BottomContent` e filtro rápido) |
| `WorkOrderDetailPage` | API + Cache + SignalR | Concluída | `AppHeader` (`ShowBackButton`, título da OS e perfil) |
| `AlertsPage` | API + Cache + SignalR | Concluída | `AppHeader` (`SegmentedControl` integrado no `BottomContent` e atualizar) |

## 4. Endpoints consumidos
| Endpoint | Usado por | Status |
| :--- | :--- | :--- |
| `POST /auth/login` | `AuthService.LoginAsync` | Integrado |
| `POST /auth/logout` | `AuthService.LogoutAsync` | Integrado (`ProfileMenuPopup`) |
| `POST /auth/forgot-password` | `AuthService.ForgotPasswordAsync` | Integrado |
| `POST /auth/reset-password` | `AuthService.ResetPasswordAsync` | Integrado |
| `GET /boards` | `DeviceService.GetDevicesAsync` | Integrado (com Cache) |
| `GET /boards/{id}` | `DeviceService.GetDeviceByIdAsync` | Integrado (com Cache) |
| `POST /boards` | `DeviceService.RegisterDeviceAsync` | Integrado |
| `GET /boards/{id}/logs` | `DeviceService.GetBoardLogsAsync` | Integrado |
| `GET /customers` | `ClientService.GetCustomersAsync` | Integrado (com Cache) |
| `GET /customers/{id}` | `ClientService.GetCustomerByIdAsync` | Integrado (com Cache) |
| `GET /maintence_requests` | `WorkOrderService.GetWorkOrdersAsync` | Integrado (com Cache) |
| `GET /maintence_requests/{id}` | `WorkOrderService.GetWorkOrderByIdAsync` | Integrado (com Cache) |
| `PUT /maintence_requests/{id}` | `WorkOrderService.CompleteWorkOrderAsync` / `SyncService` | Integrado (com Fila Offline) |
| `GET /failures` | `AlertService.GetAlertsAsync` | Integrado (com Cache) |
| `WS /hubs/telemetry` | `TelemetryHubService` | Integrado |

## 5. Mocks e hardcodes restantes
- `Services/Mocks/MockDeviceProvisioningService.cs`: provisionamento Wi-Fi ESP32 simulado (mantido conforme contrato).
- `Helpers/ApiConfig.cs`: `DefaultBaseUrl` ajustado para porta `5084` (`http://10.0.2.2:5084` em `DEBUG`).
- `ViewModels/DeviceConnectionViewModel.cs:31-37`: SSID/canal e MAC inicial padrão para o mock de provisionamento.

## 6. Decisões e desvios (Figma/API)
- `network_security_config.xml`: adicionado em `Platforms/Android/Resources/xml/` permitindo tráfego HTTP cleartext e certificados de usuário/sistema para depuração.
- `ApiClient.BuildUrl`: saneamento automático para evitar URLs duplicadas como `http://.../api/api/auth/login`.

## 7. Bugs e pendências
- 0 erros e 0 avisos no build.

## 8. Resultado do build
- `dotnet build Venther.App.csproj -f net10.0-android -r android-arm64 -c Debug`: **Sucesso** (0 erros, 0 avisos).

## 9. Próximo passo
- Reiniciar o app no dispositivo móvel apontando para o IP do host e testar a autenticação.
