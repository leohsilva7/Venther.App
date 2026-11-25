# Relatório Técnico de Estado do Projeto — Venther.App

**Data de geração:** 19/11/2026  
**Ambiente:** .NET MAUI (Android)  
**Branch:** feature/mobile-project-setup  

---

## 1. Visão Geral

- **Objetivo do App:** Aplicativo mobile para monitoramento e manutenção preventiva/corretiva de sistemas de climatização (HVAC/IoT) voltado para técnicos de campo e clientes.
- **Framework & Plataforma:** .NET 10.0 (`net10.0-android`), `SupportedOSPlatformVersion = 21.0`. Alvo único configurado no csproj.
- **Padrão Arquitetural:** MVVM com .NET MAUI Shell e injeção de dependência nativa do ASP.NET Core (`Microsoft.Extensions.DependencyInjection`).
- **Pacotes NuGet Instalados:**
  - `CommunityToolkit.Maui` (v10.0.0)
  - `CommunityToolkit.Mvvm` (v8.4.2)
  - `Microsoft.Maui.Core` (v10.0.20)
  - `Microsoft.Maui.Controls` (`$(MauiVersion)`)
  - `Microsoft.Extensions.Logging.Debug` (v10.0.0)
- **Resultado do `dotnet build`:**
  - **Status:** Compilação com êxito (**0 Erros**, **4 Avisos**).
  - **Avisos CS0618 (obsoletos):**
    - `Controls/WorkOrderCard.xaml.cs:97-98`: uso de `ScaleTo` em vez de `ScaleToAsync`.
    - `ViewModels/DeviceDetailViewModel.cs:106,110`: uso de `Shell.Current.DisplayAlert` em vez de `DisplayAlertAsync`.

---

## 2. Estrutura de Pastas e Arquivos

### Árvore de Pastas
```
Venther.App/
├── .agents/rules/venther-mobile.md
├── AGENTS.md
├── App.xaml / App.xaml.cs
├── AppShell.xaml / AppShell.xaml.cs
├── Controls/
│   ├── AlertCard.xaml / .cs
│   ├── AppEntry.xaml / .cs
│   ├── AppHeader.xaml / .cs
│   ├── DeviceCard.xaml / .cs
│   ├── MetricCard.xaml / .cs
│   ├── SegmentedControl.xaml / .cs
│   ├── SegmentItem.cs
│   ├── StatusBadge.xaml / .cs
│   ├── StepProgress.xaml / .cs
│   └── WorkOrderCard.xaml / .cs
├── Converters/ (.gitkeep)
├── Helpers/
│   ├── Routes.cs
│   ├── TrendChartDrawable.cs
│   └── Validators.cs
├── MauiProgram.cs
├── Models/
│   ├── AlertItem.cs / AlertSeverity.cs
│   ├── Device.cs / DeviceStatus.cs / DeviceTelemetry.cs
│   └── WorkOrder.cs
├── Resources/
│   ├── Fonts/ (Inter Regular/Medium/SemiBold/Bold, OpenSans)
│   ├── Images/ (21 SVGs Lucide, 2 PNGs)
│   ├── Styles/ (Colors.xaml, Styles.xaml)
│   └── AppIcon/ e Splash/
├── Services/
│   ├── Interfaces/ (IAlertService, IAuthService, IDeviceProvisioningService, IDeviceService, IWorkOrderService)
│   └── Mocks/ (MockAlertService, MockAuthService, MockDeviceProvisioningService, MockDeviceService, MockWorkOrderService)
├── ViewModels/ (14 ViewModels herdando de BaseViewModel)
└── Views/
    ├── Alerts/ (AlertsPage)
    ├── Auth/ (LoginPage, ForgotPasswordPage, CheckEmailPage, ResetPasswordPage, PasswordResetSuccessPage)
    ├── Devices/ (DeviceDetailPage, RegisterDevicePage, DeviceConnectionPage, DeviceActivatedPage)
    ├── Home/ (HomePage)
    └── WorkOrders/ (WorkOrdersPage, WorkOrderDetailPage)
```

### Tabela de Arquivos e Responsabilidades

| Arquivo | Responsabilidade | Status |
|---|---|---|
| `App.xaml` / `.cs` | Inicialização do app e fusão dos dicionários de recursos | Completo |
| `AppShell.xaml` / `.cs` | Estrutura de abas e registro de rotas modais do Shell | Completo |
| `MauiProgram.cs` | Bootstrapper, registro de fontes, DI e handlers de plataforma | Completo |
| `Controls/AlertCard.xaml` / `.cs` | Card de alerta com variantes crítico, aviso e resolvido | Completo |
| `Controls/AppEntry.xaml` / `.cs` | Campo de entrada com foco visual, erro, ícone e toggle de senha | Completo |
| `Controls/AppHeader.xaml` / `.cs` | Cabeçalho global com logo/título, ações de filtro, sync e sino | Parcial (falta botão voltar) |
| `Controls/DeviceCard.xaml` / `.cs` | Card de equipamento com barra lateral e badge de status | Completo |
| `Controls/MetricCard.xaml` / `.cs` | Card de métrica de telemetria com valor, unidade e tendência | Completo |
| `Controls/SegmentedControl.xaml` / `.cs` | Seletor horizontal em pílula com contadores dinâmicos | Completo |
| `Controls/SegmentItem.cs` | Modelo de dados para abas do SegmentedControl | Completo |
| `Controls/StatusBadge.xaml` / `.cs` | Badge com indicador luminoso (bolinha) e texto de status | Completo |
| `Controls/StepProgress.xaml` / `.cs` | Indicador de progresso com texto, percentual e barra | Completo |
| `Controls/WorkOrderCard.xaml` / `.cs` | Card de chamado (variantes em andamento pulsante e agendada) | Completo |
| `Helpers/Routes.cs` | Constantes de rotas da aplicação | Completo |
| `Helpers/TrendChartDrawable.cs` | Gráfico customizado com IDrawable, grade e curva Bézier | Completo |
| `Helpers/Validators.cs` | Validações de regex de e-mail e requisitos de senha | Completo |
| `Models/AlertItem.cs` / `AlertSeverity.cs` | Modelo de alertas do sistema | Completo |
| `Models/Device.cs` / `DeviceStatus.cs` | Modelo de equipamento e enum de status | Completo |
| `Models/DeviceTelemetry.cs` | Modelo de telemetria em tempo real e histórico | Completo |
| `Models/WorkOrder.cs` | Modelo de ordens de serviço com subtipos e status | Completo |
| `Resources/Styles/Colors.xaml` | Tokens oficiais de cores da identidade visual | Completo |
| `Resources/Styles/Styles.xaml` | Estilos base para controles, botões, cards e tipografia | Completo |
| `Services/Interfaces/*.cs` | Interfaces para os 5 domínios da aplicação | Completo |
| `Services/Mocks/*.cs` | Implementações estáticas mock para testes offline | Completo |
| `ViewModels/BaseViewModel.cs` | ViewModel base com `IsBusy` e `Title` | Completo |
| `ViewModels/LoginViewModel.cs` | Lógica de login com validação e autenticação mock | Completo |
| `ViewModels/ForgotPasswordViewModel.cs` | Lógica de recuperação de senha e validação de e-mail | Completo |
| `ViewModels/CheckEmailViewModel.cs` | ViewModel da tela de confirmação de envio de e-mail | Placeholder |
| `ViewModels/ResetPasswordViewModel.cs` | ViewModel de definição de nova senha | Placeholder |
| `ViewModels/PasswordResetSuccessViewModel.cs` | ViewModel de sucesso na redefinição de senha | Placeholder |
| `ViewModels/HomeViewModel.cs` | Dashboard de frota, filtros rápidos e busca | Completo |
| `ViewModels/AlertsViewModel.cs` | Filtragem por severidade e contadores de alertas | Completo |
| `ViewModels/WorkOrdersViewModel.cs` | Filtragem de chamados e ações de ordens de serviço | Completo |
| `ViewModels/WorkOrderDetailViewModel.cs` | ViewModel de detalhes da ordem de serviço | Placeholder |
| `ViewModels/DeviceDetailViewModel.cs` | Telemetria em tempo real e controle do gráfico | Parcial |
| `ViewModels/RegisterDeviceViewModel.cs` | Formulário de cadastro de equipamento e chips de cliente | Completo |
| `ViewModels/DeviceConnectionViewModel.cs` | Fluxo de conexão Wi-Fi e simulação de provisionamento | Completo |
| `ViewModels/DeviceActivatedViewModel.cs` | Confirmação de ativação e resumo do dispositivo | Completo |
| `Views/Alerts/AlertsPage.xaml` / `.cs` | Tela de listagem de alertas | Completo |
| `Views/Auth/LoginPage.xaml` / `.cs` | Tela de login corporativo | Completo |
| `Views/Auth/ForgotPasswordPage.xaml` / `.cs` | Tela de solicitação de recuperação de senha | Completo |
| `Views/Auth/CheckEmailPage.xaml` / `.cs` | Tela informativa de verificação de caixa postal | Placeholder |
| `Views/Auth/ResetPasswordPage.xaml` / `.cs` | Tela de digitação de nova senha | Placeholder |
| `Views/Auth/PasswordResetSuccessPage.xaml` / `.cs` | Tela de confirmação de senha redefinida | Placeholder |
| `Views/Devices/DeviceDetailPage.xaml` / `.cs` | Detalhes do equipamento, telemetria e gráfico | Parcial |
| `Views/Devices/RegisterDevicePage.xaml` / `.cs` | Passo 1 do cadastro de equipamento | Completo |
| `Views/Devices/DeviceConnectionPage.xaml` / `.cs` | Passo 2: Conexão e Wi-Fi do nó IoT | Completo |
| `Views/Devices/DeviceActivatedPage.xaml` / `.cs` | Passo 3: Confirmação e resumo de ativação | Completo |
| `Views/Home/HomePage.xaml` / `.cs` | Tela principal com frota, busca e FAB | Completo |
| `Views/WorkOrders/WorkOrdersPage.xaml` / `.cs` | Listagem e filtros de chamados de manutenção | Completo |
| `Views/WorkOrders/WorkOrderDetailPage.xaml` / `.cs` | Tela de detalhes do chamado | Placeholder |

---

## 3. Telas

| Tela Figma | Arquivos | Status | Navega para | Recebe Parâmetros | Observações / Divergências |
|---|---|---|---|---|---|
| **Login** | `LoginPage.xaml` / `.cs`, `LoginViewModel.cs` | Implementada | `//main/home`, `ForgotPasswordPage` | Não | Fiel ao Figma. Trata erro inline e card de credenciais inválidas. |
| **Forgot Password** | `ForgotPasswordPage.xaml` / `.cs`, `ForgotPasswordViewModel.cs` | Implementada | `CheckEmailPage`, `..` (Login) | Não | Validação com `Validators.IsValidEmail`. Reúsa `AuthCard`. |
| **Check Email** | `CheckEmailPage.xaml` / `.cs`, `CheckEmailViewModel.cs` | Placeholder | Nenhuma | Sim (`Email`) | Apenas label centralizado de template. |
| **Reset Password** | `ResetPasswordPage.xaml` / `.cs`, `ResetPasswordViewModel.cs` | Placeholder | Nenhuma | Não verificado | Apenas label centralizado de template. |
| **Password Reset Success** | `PasswordResetSuccessPage.xaml` / `.cs`, `PasswordResetSuccessViewModel.cs` | Placeholder | Nenhuma | Não verificado | Apenas label centralizado de template. |
| **Home (Início & Frota)** | `HomePage.xaml` / `.cs`, `HomeViewModel.cs` | Implementada | `DeviceDetailPage`, `RegisterDevicePage`, `//main/alerts` | Não | Busca reativa, chips de resumo (Total/Normal/Atenção) e FAB. |
| **Alerts** | `AlertsPage.xaml` / `.cs`, `AlertsViewModel.cs` | Implementada | `DeviceDetailPage` | Não | Filtro segmented por aba com contagem dinâmica. |
| **WorkOrders** | `WorkOrdersPage.xaml` / `.cs`, `WorkOrdersViewModel.cs` | Implementada | `WorkOrderDetailPage` | Não | Filtro por status (Em andamento, Agendada, Concluída). Botão filtro com TODO. |
| **WorkOrderDetail** | `WorkOrderDetailPage.xaml` / `.cs`, `WorkOrderDetailViewModel.cs` | Placeholder | Nenhuma | Sim (`WorkOrderId`, `WorkOrder`) | Apenas label centralizado de template. |
| **DeviceDetail** | `DeviceDetailPage.xaml` / `.cs`, `DeviceDetailViewModel.cs` | Parcial | `..` (Voltar) | Sim (`id` vs `DeviceId`) | **Falha de parâmetro:** ViewModel espera `id`, mas os chamadores passam `DeviceId` ou `AlertId`. Contém tokens inexistentes (`Success`, etc.) e 10 ícones ausentes. |
| **RegisterDevice** | `RegisterDevicePage.xaml` / `.cs`, `RegisterDeviceViewModel.cs` | Implementada | `DeviceConnectionPage`, `..` | Não | Passo 1: sugestões de clientes e validação de liberação do botão Avançar. |
| **DeviceConnection** | `DeviceConnectionPage.xaml` / `.cs`, `DeviceConnectionViewModel.cs` | Implementada | `DeviceActivatedPage`, `..` | Sim (`ClientName`, `InstallationLocation`) | Passo 2: hardware pill, Wi-Fi card e simulação de teste de 2 s via mock. |
| **DeviceActivated** | `DeviceActivatedPage.xaml` / `.cs`, `DeviceActivatedViewModel.cs` | Implementada | `//main/home`, `RegisterDevicePage`, `..` | Sim (`ClientName`, `InstallationLocation`, `HardwareId`) | Passo 3: hero card com círculos decorativos, resumo em 3 linhas e botões de retorno. |

---

## 4. Controles Reutilizáveis (`Controls/`)

| Controle | Propriedades Bindáveis | Onde é usado | Avaliação de Duplicação / Oportunidade |
|---|---|---|---|
| **`AlertCard`** | `Alert` (`AlertItem`), `ActionCommand` (`ICommand`) | `AlertsPage` | Específico e bem isolado. |
| **`AppEntry`** | `Text`, `Placeholder`, `Label`, `IsPassword`, `Keyboard`, `ReturnType`, `HasError`, `ErrorMessage`, `LeadingIconSource` | `LoginPage`, `ForgotPasswordPage`, `RegisterDevicePage`, `DeviceConnectionPage` | Excelente reutilização com estados de foco, erro e senha. |
| **`AppHeader`** | `Title`, `ShowSyncButton`, `ShowBellButton`, `ShowFilterButton`, `SyncCommand`, `BellCommand`, `FilterCommand` | `HomePage`, `AlertsPage`, `WorkOrdersPage` | **Duplicação detectada:** As telas `RegisterDevicePage`, `DeviceConnectionPage`, `DeviceActivatedPage` e `DeviceDetailPage` recriaram cabeçalhos manuais com botão voltar porque `AppHeader` não tem `ShowBackButton`. |
| **`DeviceCard`** | `Device` (`Device`), `TapCommand` (`ICommand`) | `HomePage` | Específico da listagem de frota. |
| **`MetricCard`** | `Title`, `Value`, `Unit`, `Caption`, `IconSource`, `TrendIconSource`, `IsAlert` | `DeviceDetailPage` | Reutilizável no grid 2x2. |
| **`SegmentedControl`** | `ItemsSource` (`IEnumerable<SegmentItem>`), `SelectedIndex` (`int`), `SelectionChangedCommand` (`ICommand`) | `AlertsPage`, `WorkOrdersPage` | Reutilizado entre alertas e chamados. |
| **`StatusBadge`** | `Status` (`DeviceStatus`) | `DeviceCard` | Reutilizável com bolinha colorida. |
| **`StepProgress`** | `CurrentStep` (`int`), `TotalSteps` (`int`) | `RegisterDevicePage`, `DeviceConnectionPage`, `DeviceActivatedPage` | Excelente reaproveitamento nos passos 1, 2 e 3 do cadastro. |
| **`WorkOrderCard`** | `WorkOrder` (`WorkOrder`), `ActionCommand` (`ICommand`) | `WorkOrdersPage` | Renderiza dinamicamente as variantes "Em Andamento" (com pulso) e "Agendada". |

---

## 5. Estilos e Design System

### Tokens em `Colors.xaml`
- `Primary` (#00326F), `Accent` (#FC6100), `Background` (#F6F9FF), `Surface` (#EEF4FC), `SurfaceVariant` (#E8EEF6), `OnSurface` (#161C22), `OnSurfaceVariant` (#424751), `Outline` (#737783), `AccentContainer` (#FFDBCD), `OnAccentContainer` (#360F00), `AccentText` (#A43D00).
- `AuthText` (#0F172A), `AuthMuted` (#64748B), `AuthBorder` (#E2E8F0), `Error` (#BA1A1A), `ErrorBg` (#FEF2F2), `ErrorBorder` (#FECACA), `LogoBlue` (#034899), `White` (#FFFFFF).

### Estilos em `Styles.xaml`
- `ContentPage` (implícito), `BaseLabel`, `Label` (implícito), `TitleAuth`, `Heading`, `BodySmall`, `Caption`, `Overline`.
- `PrimaryButton`, `LinkButton`, `Card` (`Border`), `AuthCard` (`Border`).

### Pontos com Cores Hardcoded ou Tokens Inexistentes fora dos arquivos de estilo

| Arquivo | Linha(s) | Código / Problema |
|---|---|---|
| `Controls/AlertCard.xaml.cs` | 56-58 | `#DCFCE7`, `#15803D`, `#16A34A` hardcoded no C# |
| `Controls/DeviceCard.xaml.cs` | 68 | `#16A34A` hardcoded no C# |
| `Controls/StatusBadge.xaml.cs` | 50-52 | `#DCFCE7`, `#16A34A`, `#15803D` hardcoded no C# |
| `Controls/WorkOrderCard.xaml.cs` | 114-118 | `#16A34A`, `#DCFCE7`, `#15803D` hardcoded no C# |
| `Helpers/TrendChartDrawable.cs` | 9-11 | `#00326F`, `#FC6100`, `#C3C6D3` hardcoded no C# |
| `Controls/MetricCard.xaml.cs` | 32 | Chama `Res("SuccessText")` que **não existe** em `Colors.xaml` (cai em fallback `Black`) |
| `Views/Devices/DeviceDetailPage.xaml` | 46, 58, 71, 75, 99, 103 | Referencia `{StaticResource Success}`, `{StaticResource SuccessContainer}`, `{StaticResource OnSuccessContainer}`, `{StaticResource OnAccent}` que **não existem** em `Colors.xaml` |
| `Views/Devices/DeviceActivatedPage.xaml` | 107, 130, 135, 144, 150, 157, 269, 274 | `#DCFCE7`, `#16A34A`, `#15803D` hardcoded no XAML |
| `Views/Devices/DeviceConnectionPage.xaml` | 86, 156, 162 | `#16A34A`, `#DCFCE7`, `#15803D` hardcoded no XAML |
| `Views/Home/HomePage.xaml` | 103 | `#16A34A` hardcoded no XAML |

### Inventário de Ícones SVG (`Resources/Images/`)
- **Presentes fisicamente (21 SVGs):** `alert_circle.svg`, `alert_triangle.svg`, `arrow_left.svg`, `bell.svg`, `calendar.svg`, `check_circle.svg`, `chevron_right.svg`, `clock.svg`, `cpu.svg`, `filter.svg`, `key_round.svg`, `map_pin.svg`, `more_vertical.svg`, `plus.svg`, `refresh_cw.svg`, `search.svg`, `shield_check.svg`, `user.svg`, `wifi.svg`, `wrench.svg`, `zap.svg`.
- **Referenciados no código mas NÃO existem no disco (10 SVGs - todos em `DeviceDetailPage.xaml`):**
  - `chevron_left.svg` (linha 14)
  - `circle_check.svg` (linha 68)
  - `download.svg` (linha 116)
  - `snowflake.svg` (linha 187)
  - `trending_down.svg` (linha 187)
  - `thermometer.svg` (linha 191)
  - `minus.svg` (linha 191)
  - `gauge.svg` (linha 195)
  - `chevrons_up_down.svg` (linha 199)
  - `check.svg` (linha 199)

### Fontes Registradas vs Existência Física
- `Inter_24pt-Regular.ttf` -> alias `InterRegular` (Presente, 342 KB)
- `Inter_24pt-Medium.ttf` -> alias `InterMedium` (Presente, 342 KB)
- `Inter_24pt-SemiBold.ttf` -> alias `InterSemiBold` (Presente, 343 KB)
- `Inter_24pt-Bold.ttf` -> alias `InterBold` (Presente, 344 KB)
- `OpenSans-Regular.ttf` -> alias `OpenSansRegular` (Presente, 107 KB)
- `OpenSans-Semibold.ttf` -> alias `OpenSansSemibold` (Presente, 111 KB)

---

## 6. Navegação

### Estrutura do AppShell
- **Abas principais (`TabBar Route="main"`):**
  - `Início` (`//main/home` -> `HomePage`)
  - `Alertas` (`//main/alerts` -> `AlertsPage`)
  - `Chamados` (`//main/workorders` -> `WorkOrdersPage`)
- **Tela fora de abas:**
  - `login` (`//login` -> `LoginPage`)
- **Rotas registradas via `Routing.RegisterRoute`:**
  - `devicedetail` (`DeviceDetailPage`)
  - `workorderdetail` (`WorkOrderDetailPage`)
  - `registerdevice` (`RegisterDevicePage`)
  - `deviceconnection` (`DeviceConnectionPage`)
  - `deviceactivated` (`DeviceActivatedPage`)
  - `forgotpassword` (`ForgotPasswordPage`)
  - `checkemail` (`CheckEmailPage`)
  - `resetpassword` (`ResetPasswordPage`)
  - `passwordresetsuccess` (`PasswordResetSuccessPage`)

### Status dos Fluxos de Navegação

| Fluxo | Caminho | Status |
|---|---|---|
| Login -> Home | `LoginPage` -> `//main/home` | Funcionando |
| Login -> Esqueci Senha | `LoginPage` -> `ForgotPasswordPage` | Funcionando |
| Esqueci Senha -> Check Email | `ForgotPasswordPage` -> `CheckEmailPage` | Funcionando (destino é placeholder) |
| Esqueci Senha -> Voltar | `ForgotPasswordPage` -> `..` | Funcionando |
| Home -> Alertas | Sino do header -> `//main/alerts` | Funcionando |
| Home -> Cadastro de Dispositivo | FAB (+) -> `RegisterDevicePage` | Funcionando |
| Home -> Detalhe do Equipamento | Card de equipamento -> `devicedetail` | **Incompleto:** parâmetro com chave divergente |
| Alertas -> Detalhe do Equipamento | Card de alerta / botão "Ver" -> `devicedetail` | **Incompleto:** parâmetro com chave divergente |
| Chamados -> Detalhe do Chamado | Card de OS -> `workorderdetail` | Funcionando (destino é placeholder) |
| Cadastro -> Conexão -> Ativação | `RegisterDevicePage` -> `DeviceConnectionPage` -> `DeviceActivatedPage` | Funcionando de ponta a ponta com parâmetros |
| Ativação -> Home / Novo Cadastro | `DeviceActivatedPage` -> `//main/home` ou `RegisterDevicePage` | Funcionando |

### Passagem de Parâmetros
- Feita via dicionário `IDictionary<string, object>` no método `Shell.Current.GoToAsync(route, parameters)`.
- Telas que recebem via `[QueryProperty]`:
  - `DeviceConnectionViewModel`: `ClientName`, `InstallationLocation`.
  - `DeviceActivatedViewModel`: `ClientName`, `InstallationLocation`, `HardwareId`.
  - `DeviceDetailViewModel`: `[QueryProperty(nameof(DeviceId), "id")]` — **Divergência:** o chamador passa `"DeviceId"` ou `"AlertId"`, fazendo com que `DeviceId` chegue nulo.

---

## 7. Dados Mockados e Simulações

| Interface | Implementação Mock | O que está mockado | Localização do Mock |
|---|---|---|---|
| `IAuthService` | `MockAuthService` | Autenticação booleana simples sem validação remota | `Services/Mocks/MockAuthService.cs` |
| `IDeviceService` | `MockDeviceService` | Lista de 4 equipamentos fixos, telemetria fixa e arrays de temperatura de 1h/12h/24h | `Services/Mocks/MockDeviceService.cs` |
| `IWorkOrderService` | `MockWorkOrderService` | Lista de 5 ordens de serviço fixas com subtipos | `Services/Mocks/MockWorkOrderService.cs` |
| `IAlertService` | `MockAlertService` | Lista de 3 alertas com severidades fixa | `Services/Mocks/MockAlertService.cs` |
| `IDeviceProvisioningService` | `MockDeviceProvisioningService` | Sucesso imediato com atraso de 2 segundos | `Services/Mocks/MockDeviceProvisioningService.cs` |

### Dados Mockados Hardcoded dentro de ViewModels (Anti-pattern)
- `ViewModels/RegisterDeviceViewModel.cs:10-18`: Lista fixa `_allClients` ("Hospital Santa Casa", "Supermercado Bom Preço", etc.) declarada internamente na ViewModel em vez de vir de um serviço `IClientService`.
- `ViewModels/DeviceConnectionViewModel.cs:20-29`: Valores padrão de hardware (`ESP32-NODE-089A`, MAC e rede Wi-Fi) inicializados diretamente nas propriedades privadas.
- `ViewModels/DeviceActivatedViewModel.cs:15-26`: Telemetria inicial (`22.4°C · Pressão 12.1 bar`) e carimbo de data/hora declarados diretamente na ViewModel.

### Simulações e Atrasos Artificiais
- `ViewModels/ForgotPasswordViewModel.cs:88`: `await Task.Delay(1500)` para simular envio de e-mail.
- `Services/Mocks/MockDeviceProvisioningService.cs:11`: `await Task.Delay(2000)` para simular BLE/Wi-Fi handshake.
- `ViewModels/LoginViewModel.cs:122`: Validação de senha fixa ("admin"/"Password123!") em código de teste.

### Levantamento de Comentários `TODO` / `FIXME`

| Arquivo | Linha | Descrição |
|---|---|---|
| `ViewModels/AlertsViewModel.cs` | 30 | Implementar `ShellBottomNavViewAppearanceTracker` nativo no Android para badge de abas |
| `ViewModels/DeviceDetailViewModel.cs` | 106 | Conectar ação de solicitação de manutenção preventiva com a API |
| `ViewModels/DeviceDetailViewModel.cs` | 110 | Conectar download do relatório PMOC (PDF) com a API |
| `ViewModels/WorkOrdersViewModel.cs` | 105, 109 | Implementar filtro avançado de chamados via bottom-sheet/modal |
| `AppShell.xaml` | 24 | Substituir ícones de abas (`dotnet_bot.png`) por SVGs Lucide |

---

## 8. Estado do MVVM e Injeção de Dependência

### Registros em `MauiProgram.cs`
- **Singletons (Serviços):** `IAuthService`, `IDeviceService`, `IWorkOrderService`, `IAlertService`, `IDeviceProvisioningService`.
- **Transients (Pages e ViewModels):** Todos os 13 pares de Telas e ViewModels estão registrados. Nenhum arquivo existente foi esquecido.

### Violações de MVVM e Responsabilidades
- **Imports de plataforma em ViewModels:**
  - `ViewModels/DeviceDetailViewModel.cs:6`: `using static Android.Telephony.CarrierConfigManager;` (vaza namespace do Android para a camada de apresentação).
- **Acesso direto à UI/Dialogs em ViewModels:**
  - `ViewModels/DeviceDetailViewModel.cs:106, 110`: chamada direta a `Shell.Current.DisplayAlert(...)`.
  - `ViewModels/DeviceActivatedViewModel.cs:61`: chamada direta a `Application.Current.Windows[0].Page!.DisplayActionSheetAsync(...)`.
- **Acoplamento direto com Navegação Shell:** Todas as ViewModels utilizam `Shell.Current.GoToAsync` diretamente sem abstração por interface `INavigationService`.

### Validações
- Classe utilitária `Helpers/Validators.cs`:
  - `IsValidEmail`: validado via `MailAddress.TryCreate`. Utilizado em `LoginViewModel` e `ForgotPasswordViewModel`.
  - `IsValidPassword`: regex com mínimo de 8 caracteres, 1 maiúscula e 1 caractere especial. Utilizado em `LoginViewModel`.

---

## 9. Preparação para a Integração com API

### Métodos e Modelos Atuais

```csharp
// IAuthService
Task<bool> LoginAsync(string email, string password);
Task LogoutAsync();
Task<bool> SendPasswordResetEmailAsync(string email);
Task<bool> ResetPasswordAsync(string token, string newPassword);
Task<bool> IsAuthenticatedAsync();

// IDeviceService
Task<IReadOnlyList<Device>> GetDevicesAsync();
Task<Device?> GetDeviceByIdAsync(string id);
Task<bool> RegisterDeviceAsync(Device device);
Task<bool> UpdateDeviceStatusAsync(string id, DeviceStatus status);
Task<DeviceTelemetry> GetTelemetryAsync(string deviceId);
Task<IReadOnlyList<double>> GetTemperatureTrendAsync(string deviceId, TrendPeriod period);

// IWorkOrderService
Task<IReadOnlyList<WorkOrder>> GetWorkOrdersAsync();
Task<WorkOrder?> GetWorkOrderByIdAsync(string id);
Task<bool> CreateWorkOrderAsync(WorkOrder workOrder);
Task<bool> CompleteWorkOrderAsync(string id);

// IAlertService
Task<IReadOnlyList<AlertItem>> GetAlertsAsync();
Task<AlertItem?> GetAlertByIdAsync(string id);
Task<bool> ResolveAlertAsync(string id);

// IDeviceProvisioningService
Task<bool> ProvisionDeviceAsync(string hardwareId, string ssid, string password);
```

### O que precisa mudar para API Real
1. **Camada de DTOs e Mappers:** Modelos atuais misturam regras de UI com dados brutos. Criar DTOs para serialização JSON com `System.Text.Json`.
2. **Refatoração de Retornos:** Substituir retornos booleanos simples (`Task<bool>`) por objetos de resultado (`Result<T>` ou tratamento com exceções tipadas de API / status HTTP).
3. **Gerenciamento de Sessão e Tokens:** Implementar interceptor `DelegatingHandler` no `HttpClient` para inclusão do header `Authorization: Bearer <token>` e renovação via Refresh Token.
4. **Armazenamento Seguro:** Uso de `SecureStorage` (do .NET MAUI) para salvar tokens JWT e credenciais de forma criptografada.
5. **Modo Offline e Cache Local (RNF12):** Implementar banco local SQLite com Entity Framework Core ou SQLite-net para persistência de frota, ordens de serviço e cache de telemetria.
6. **Comunicação em Tempo Real:** Integração de cliente SignalR (`Microsoft.AspNetCore.SignalR.Client`) para streaming de telemetria na `DeviceDetailPage` e recebimento de alertas em tempo de execução.
7. **Notificações Push:** Configuração do Firebase Cloud Messaging (FCM) para Android.

---

## 10. Escalabilidade e Qualidade do Código

### Notas de Avaliação (1 a 5)

| Dimensão | Nota | Justificativa |
|---|:---:|---|
| **Organização de Pastas** | 5/5 | Estrutura limpa, padronizada e modular (separação de Views por domínio, Controls, Models, Services). |
| **Separação de Responsabilidades** | 4/5 | MVVM respeitado na maioria das telas; pequenos desvios em chamadas diretas de diálogos em ViewModels. |
| **Reaproveitamento de Código** | 4/5 | Controles reutilizáveis bem construídos (`AppEntry`, `StepProgress`, cards). Oportunidade não aproveitada em cabeçalhos de tela. |
| **Consistência de Nomenclatura** | 5/5 | 100% aderente ao AGENTS.md: código em inglês (PascalCase/camelCase) e UI em português do Brasil. |
| **Tratamento de Erros** | 3/5 | Erros visuais de validação bem tratados; faltam políticas de resiliência de rede e tratamento global de exceções. |
| **Acessibilidade (SemanticProperties)** | 2/5 | Apenas alguns botões e imagens possuem `SemanticProperties.Description`. Leitor de tela terá navegação deficiente. |
| **Performance** | 4/5 | Uso de `CollectionView`, `Border`, SourceGen habilitado no projeto e SVGs nativos. |
| **Testabilidade** | 3.5/5 | Serviços desacoplados por interfaces facilitam testes unitários; presença de chamadas estáticas `Shell.Current` reduz testabilidade das ViewModels. |

**Média Geral:** **3.8 / 5.0**

### 10 Maiores Riscos e Dívidas Técnicas

| # | Problema | Onde | Impacto | Como Corrigir |
|---|---|---|---|---|
| **1** | **Tokens de cor inexistentes no dicionário de recursos** | `DeviceDetailPage.xaml:46,58,71,99` | Falha de estilização ou crash em tempo de execução ao abrir tela | Adicionar tokens `Success`, `SuccessContainer`, `OnSuccessContainer` e `OnAccent` em `Colors.xaml` ou substituir pelos tokens oficiais. |
| **2** | **10 ícones SVG referenciados que não existem no disco** | `DeviceDetailPage.xaml:14,68,116,187,191,195,199` | Imagens vazias / ausência visual de ícones de telemetria | Adicionar os SVGs faltantes em `Resources/Images/` ou remapear para ícones existentes. |
| **3** | **Parâmetro de navegação com chave divergente** | `DeviceDetailViewModel.cs:15` vs chamadores (`DeviceCard`, `AlertCard`) | Tela de detalhes abre sem carregar o equipamento selecionado | Unificar chave de query para `"id"` ou `"DeviceId"` em ambos os lados. |
| **4** | **Import acidental de namespace Android na ViewModel** | `DeviceDetailViewModel.cs:6` (`using static Android.Telephony...`) | Poluição de namespace e quebra conceitual de código multiplataforma | Remover o using desnecessário. |
| **5** | **Duplicação de cabeçalhos com botão voltar** | `DeviceDetailPage`, `RegisterDevicePage`, `DeviceConnectionPage`, `DeviceActivatedPage` | Inconsistência visual e retrabalho de manutenção | Adicionar propriedade `ShowBackButton` e `BackCommand` no `AppHeader` e utilizá-lo nessas telas. |
| **6** | **Dados de clientes hardcoded dentro da ViewModel** | `RegisterDeviceViewModel.cs:10-18` | Impossibilidade de sincronizar com a base de clientes do backend | Mover a lista de clientes para interface de serviço (`IClientService` ou `IDeviceService`). |
| **7** | **Cores hexadecimais hardcoded fora dos tokens de estilo** | `DeviceActivatedPage`, `DeviceConnectionPage`, `WorkOrderCard.cs`, `AlertCard.cs`, etc. | Dificuldade para suportar Dark Mode e quebra do Design System | Criar tokens semânticos de verde (ex: `Success`, `SuccessBg`, `SuccessText`) em `Colors.xaml`. |
| **8** | **Ícones das abas do Shell usando template padrão** | `AppShell.xaml:25,28,31` | Identidade visual incompleta na barra inferior de navegação | Substituir `dotnet_bot.png` pelos SVGs Lucide apropriados. |
| **9** | **Chamada direta de UI/Dialogs nas ViewModels** | `DeviceDetailViewModel.cs:106`, `DeviceActivatedViewModel.cs:61` | Dificulta testes unitários automatizados | Criar abstração `IDialogService` e injetar nas ViewModels. |
| **10** | **Quatro telas do fluxo de autenticação e OS como placeholder** | `CheckEmailPage`, `ResetPasswordPage`, `PasswordResetSuccessPage`, `WorkOrderDetailPage` | Fluxos de recuperação de senha e gestão de ordens incompletos | Implementar XAML e ViewModels finais das páginas pendentes. |

---

## 11. Divergências em Relação ao Figma e Requisitos

### Divergências Visuais / Estruturais Observadas
1. **Ícone das Abas:** O Figma prevê ícones de linha limpos para Home, Alertas e Chamados; o app ainda utiliza o ícone de template `dotnet_bot.png`.
2. **Badge de Notificações na TabBar:** Figma exibe badge numérico na aba "Alertas"; funcionalidade não implementada nativamente (marcada com TODO).
3. **Filtro Avançado de Chamados:** O botão de filtro no cabeçalho de `WorkOrdersPage` não abre a gaveta/bottom-sheet de ordenação.
4. **Histórico na Tela de Detalhes:** Aba "Histórico" de `DeviceDetailPage` possui apenas um label placeholder avisando indisponibilidade.

### Requisitos e Funcionalidades Previstas sem Tela
- **Perfil do Técnico / Usuário:** Não há tela de perfil, dados da conta, configurações ou logout (apenas avatar estático no header).
- **Solicitação de Manutenção pelo Cliente:** Botão existente emite apenas `DisplayAlert` mockado; não há formulário de abertura de chamado.
- **Relatório PMOC em PDF:** Botão existente emite apenas alerta informativo.
- **Gerenciamento de Notificações Push:** Sem interface para preferências de alertas críticos vs informativos.

---

## 12. Próximos Passos

### Fase 1: Estabilização e Conclusão do Frontend (UI/UX)
1. **Corrigir pendências críticas de `DeviceDetailPage`:** Adicionar os 10 ícones SVG faltantes, cadastrar tokens de sucesso em `Colors.xaml` e unificar a chave de navegação `DeviceId`. *(Esforço: P)*
2. **Implementar as 3 telas pendentes do fluxo de senha:** `CheckEmailPage`, `ResetPasswordPage` e `PasswordResetSuccessPage` com base no Figma e reutilizando `AuthCard` e `AppEntry`. *(Esforço: M)*
3. **Implementar a tela de detalhes de chamado:** Desenvolver `WorkOrderDetailPage` com status, histórico e checklist de serviços do técnico. *(Esforço: M)*
4. **Evoluir o `AppHeader`:** Adicionar suporte nativo a `ShowBackButton` e `BackCommand`, eliminando as implementações manuais de cabeçalho. *(Esforço: P)*
5. **Ajustar ícones nativos das abas do Shell:** Inserir os SVGs oficiais na `TabBar`. *(Esforço: P)*

### Fase 2: Preparação para Integração com API
1. **Implementar infraestrutura HTTP:** Criar `ApiClient` com `HttpClientFactory`, interceptor de autenticação Bearer e serializador JSON resiliente. *(Esforço: M)*
2. **Implementar persistência segura e sessão:** Criar `IAuthStorageService` utilizando `SecureStorage` para guardar tokens JWT. *(Esforço: P)*
3. **Substituir mocks por clientes de API reais:** Implementar versões de produção de `AuthService`, `DeviceService`, `WorkOrderService` e `AlertService`. *(Esforço: G)*
4. **Configurar SignalR:** Implementar hub de conexão para atualização contínua dos cards de telemetria e recepção de alertas instantâneos. *(Esforço: M)*
5. **Configurar banco de dados local SQLite (RNF12):** Implementar cache offline para leitura da frota e ordens mesmo em áreas sem sinal de internet. *(Esforço: G)*
