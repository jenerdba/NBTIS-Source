--Primary
alter table nbi.BridgePrimary
alter column OvertopLikelihood_BAP02 varchar(5) null

alter table nbi.BridgePrimary
alter column ScourVulnerability_BAP03 varchar(5) null

alter table nbi.BridgePrimary
alter column ScourPOA_BAP04 varchar(3) null

alter table nbi.BridgePrimary
alter column ScourCondRate_BC11 varchar(3) null

alter table nbi.Stage_BridgePrimary
alter column OvertopLikelihood_BAP02 varchar(5) null

alter table nbi.Stage_BridgePrimary
alter column ScourVulnerability_BAP03 varchar(5) null

alter table nbi.Stage_BridgePrimary
alter column ScourPOA_BAP04 varchar(3) null

alter table nbi.Stage_BridgePrimary
alter column ScourCondRate_BC11 varchar(3) null

--Features
alter table nbi.BridgeFeatures
alter column FuncClass_BH01 varchar(3) null

alter table nbi.BridgeFeatures
alter column NatHwyFreightNet_BH04 varchar(3) null

alter table nbi.Stage_BridgeFeatures
alter column FuncClass_BH01 varchar(3) null

alter table nbi.Stage_BridgeFeatures
alter column NatHwyFreightNet_BH04 varchar(3) null

--Routes
alter table nbi.BridgeRoutes
alter column RouteDirection_BRT03 varchar(3) null

alter table nbi.Stage_BridgeRoutes
alter column RouteDirection_BRT03 varchar(3) null

--SpanSets
alter table nbi.Stage_BridgeSpanSets
alter column SpanMaterial_BSP04 varchar(4) null

alter table nbi.BridgeSpanSets
alter column SpanMaterial_BSP04 varchar(4) null
