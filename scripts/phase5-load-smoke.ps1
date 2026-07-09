param(
    [Parameter(Mandatory)] [string] $BaseUrl,
    [Parameter(Mandatory)] [string] $Token,
    [Parameter(Mandatory)] [Guid] $OrganizationId,
    [Parameter(Mandatory)] [Guid] $BranchId,
    [int] $Requests = 100,
    [int] $Concurrency = 10,
    [int] $P95LimitMs = 1500
)

$client = [System.Net.Http.HttpClient]::new()
$client.DefaultRequestHeaders.Authorization = [System.Net.Http.Headers.AuthenticationHeaderValue]::new("Bearer", $Token)
$client.DefaultRequestHeaders.Add("X-Branch-ID", $BranchId.ToString())
$to = [DateTime]::UtcNow.ToString("yyyy-MM-dd")
$from = [DateTime]::UtcNow.AddDays(-29).ToString("yyyy-MM-dd")
$uri = "$($BaseUrl.TrimEnd('/'))/v2/api/reports/financial/$OrganizationId?fromDate=$from&toDate=$to&branchId=$BranchId"
$durations = [System.Collections.Generic.List[double]]::new()
$failures = 0

for ($offset = 0; $offset -lt $Requests; $offset += $Concurrency) {
    $batchSize = [Math]::Min($Concurrency, $Requests - $offset)
    $timers = 1..$batchSize | ForEach-Object { [System.Diagnostics.Stopwatch]::StartNew() }
    $tasks = 1..$batchSize | ForEach-Object { $client.GetAsync($uri) }
    [System.Threading.Tasks.Task]::WaitAll([System.Threading.Tasks.Task[]] $tasks)
    for ($index = 0; $index -lt $batchSize; $index++) {
        $timers[$index].Stop()
        $durations.Add($timers[$index].Elapsed.TotalMilliseconds)
        if (-not $tasks[$index].Result.IsSuccessStatusCode) { $failures++ }
        $tasks[$index].Result.Dispose()
    }
}

$ordered = $durations | Sort-Object
$p95 = $ordered[[Math]::Min($ordered.Count - 1, [Math]::Ceiling($ordered.Count * 0.95) - 1)]
$average = ($durations | Measure-Object -Average).Average
Write-Output "requests=$Requests failures=$failures average_ms=$([Math]::Round($average, 1)) p95_ms=$([Math]::Round($p95, 1)) limit_ms=$P95LimitMs"
$client.Dispose()
if ($failures -gt 0 -or $p95 -gt $P95LimitMs) { exit 1 }
