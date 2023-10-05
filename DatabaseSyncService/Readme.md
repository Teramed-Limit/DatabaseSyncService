Database Sync Service
===

## 專案介紹

利用開啟DB Change Tracking功能，並透過SQL Server Agent Job定期執行，將變更的資料同步至目標資料庫。

## 目錄

- [安裝](#安裝)
- [卸除](#卸除)
- [設定](#AppSettings)

## 安裝

以系統管理員執行`create-service.bat`

## 卸除

以系統管理員執行`delete-service.bat`

## AppSettings

#### SourceDBSettings（源數據庫設置）

- `ServerName`（服務器名稱）：源數據庫的服務器名稱。設置為 "localhost,1433"。
- `DatabaseName`（數據庫名稱）：源數據庫的名稱。設置為 "ReportServer"（報告服務器）。
- `UserName`（用戶名）：用於連接到源數據庫的用戶名。設置為 "sa"。
- `Password`（密碼）：用於連接到源數據庫的密碼。設置為 "admin"。

#### TargetDBSettings（目標數據庫設置）

- `ServerName`（服務器名稱）：目標數據庫的服務器名稱。設置為 "localhost,1443"。
- `DatabaseName`（數據庫名稱）：目標數據庫的名稱。設置為 "ReportServer"（報告服務器）。
- `UserName`（用戶名）：用於連接到目標數據庫的用戶名。設置為 "sa"。
- `Password`（密碼）：用於連接到目標數據庫的密碼。設置為 "yourStrong(!)Password"（您的強密碼）。

#### TableSyncMode（表同步模式）

- `All`（全部）：指定表同步模式。全部資料表都開啟DB Change Tracking功能。
- `Include`（包含）：指定表同步模式。包含指定的表。和TableNames（表名稱）一起使用。
- `Exclude`（排除）：指定表同步模式。排除指定的表。和TableNames（表名稱）一起使用。

#### TableNames（表名稱）

- `字串陣列`：根據TableSyncMode（表同步模式）指定的模式，指定要同步的Table
    - `TableSyncMode` 為 `All`（全部）時，全部資料表都開啟DB Change Tracking功能並同步。
    - `TableSyncMode` 為 `Include`（包含）時，只有陣列內的Table會開啟DB Change Tracking功能並同步。
    - `TableSyncMode` 為 `Exclude`（排除）時，排除陣列內的Table，其餘Table都會開啟DB Change Tracking功能並同步。

#### BackupToTargetDB（備份至目標數據庫）

主服務器會定時同步到備份服務器，當主服務發生異常時，備份服務可以立即啟動並提供服務。
但因主服務器尚未上線，所以這邊可以設定是否要同步。

- `true`: 會同步備份到目標數據庫。
- `false`: 不會同步備份到目標數據庫。但會開啟DB Change Tracking功能。

#### ChangeRetentionDays（更改保留天數）

- `number`：Change Tracking功能會保留2天內的變更資料。其餘資料會被刪除。

#### SyncIntervalSeconds（同步間隔秒數）

主服務器可以設定間隔少一點，因為主服務器會同步到備份服務器
備份服務器可以設定間隔多一點，因為主服務器不知道多久才會重新上線，等到主服務器上線時，備份服務器會同步到主服務器。

- `number`：每隔幾秒鐘同步一次資料。
