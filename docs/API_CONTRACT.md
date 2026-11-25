# Contrato da API Venther (resumo para o app mobile)
Base: {BaseUrl}/api. JSON camelCase. Header Authorization: Bearer <token>. Erro: {"error":"msg"} (400/401/403/404/409/500); mensagens da API vêm em inglês, o app mostra PT-BR.
Papéis: Admin e Technician (o app é do Technician). Sem refresh token: o JWT dura 8h; em 401 limpar sessão e ir ao login.
Paginação: ?page=1&pageSize=10 (máx 100) => {items[],page,pageSize,totalCount,totalPages}.

AUTH
POST /auth/login {email,password} -> {token,expiresIn(seg),user{id,name,email,role,phoneNumber,createdAt}}
POST /auth/logout -> 204
POST /auth/forgot-password {email} -> {message}. Envia código de 6 dígitos por e-mail (expira em 5 min)
POST /auth/reset-password {email,code,password,confirmPassword} -> {message}. 400: "Invalid or expired reset code." / "Passwords do not match."
Senha: 8-100 caracteres, com maiúscula, minúscula, número e caractere especial.

CUSTOMERS
GET /customers (paginado) -> {id,name,email,location,phoneNumber,createdAt}
GET /customers/{guid} -> idem + boards[{id,macAddress,location,installationDate}]

BOARDS (placa = equipamento)
GET /boards (paginado) -> {id,macAddress,customerId,customerName,installationDate,location,createdAt}
GET /boards/{guid}
POST /boards {macAddress,customerId,installationDate("yyyy-MM-dd"),location(máx 120)} -> 201. 409 = MAC duplicado. MAC no formato AA:BB:CC:DD:EE:FF
GET /boards/{guid}/logs?date=yyyy-MM -> [{id,boardId,supplyAirTemperature,returnAirTemperature,condensationAirTemperature,spaceTemperature,gasLineTemperature,liquidLineTemperature,evaporatorCoilPressureDrop,condenserCoilPressureDrop,createdAt}] (doc: só Admin; no app tratar 401/403/404 com fallback)

FAILURES (doc: só Admin)
GET /failures?page&pageSize&boardId -> paginado {id(int),boardId,boardMacAddress,date,failureCause,reportId,maintenanceRequestId}

MAINTENANCE REQUESTS (rota /maintence_requests; alias /maintenance-requests)
GET /maintence_requests?page&pageSize&boardId&userId&isDone -> paginado {id(int),boardId,boardMacAddress,boardLocation,userId,userName,userEmail,date("yyyy-MM-dd"),isDone}
GET /maintence_requests/{int}
PUT /maintence_requests/{int} body {"isDone":true} (técnico só pode isso) -> 204

SIGNALR hub {BaseUrl}/hubs/telemetry (token via accessTokenFactory)
invoke SubscribeToBoards(Guid[]) -> BoardTelemetryDto[] (estado atual); UnsubscribeFromBoards(Guid[])
Eventos: TelemetryUpdated(BoardTelemetryDto); FaultRaised({boardId,failureCause,date})
BoardTelemetryDto: boardId, macAddress, supplyAirTemperature, returnAirTemperature, condensationAirTemperature, spaceTemperature, gasLineTemperature, liquidLineTemperature, evaporatorCoilPressureDrop, condenserCoilPressureDrop, isOnline, lastSeen

DECISÕES DE MAPEAMENTO
- Device: Id=board.id, Name=board.location, Location=customerName, HardwareId=macAddress. A Home lista todas as placas (GET /boards).
- Status do device: até a etapa 5 = Normal. Depois: online=Normal, offline ou sem leitura=Attention, FaultRaised=Critical.
- Cards de telemetria: Temp Insuflamento=supplyAirTemperature; Temp Retorno=returnAirTemperature; o card "Pressão Gás" vira "Temp. Linha Gás"=gasLineTemperature (°C), porque a API não tem pressão em bar; Delta P Evap.=evaporatorCoilPressureDrop (Pa). Legenda "Ao vivo" ou "Offline", e IsAlert=!isOnline. Não inventar faixas de normalidade (o diagnóstico é do backend).
- WorkOrder: Title="Manutenção preventiva", DeviceName=boardLocation. Status: isDone=Concluída; date>hoje=Agendada; senão Em andamento. Filtrar por userId do usuário logado. Checklist de peças é visual/local.
- Alert: Title=failureCause, DeviceInfo=location da placa (ou MAC), Severity=Critical, IsResolved=false.
- Tendência térmica: buffer do SignalR (supplyAirTemperature) + logs do mês quando permitido.
- Provisionamento Wi-Fi do ESP32 continua MOCK; o MAC é digitado em campo na etapa 2 do cadastro.
- Fora do escopo agora: modo offline/SQLite, push, relatório PMOC, "Solicitar manutenção" (POST é só Admin: manter botão com aviso "Em breve").