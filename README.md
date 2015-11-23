# AzLog
Azure diagnostics log viewer

The inspiration for this came from my needing to look at logs that were uploaded to Azure table storage
using the default Azure diagnostic listener. I had a bunch of log data uploaded to Azure table storage, but 
no really convenient way to read it.  (Well, I'm sure there were many already out there, but I needed a new
programming distraction).

I have lofty goals for this over time, like being able to open data sources other than Azure and allowing 
you to create mappings of columns from the source data into the log format in memory (allowing you to create
"merged" views of logs -- very handy when you have a correlation ID that you want to filter by in order to
see operation across client and server).

For now, this is fairly basic (and the UI is lame, which reflects my lack of UI design skill -- anyone care
to contribute here?)

After running AzLog, add an account using Add/Edit Account. You will need your storage account name and your
account key. I've only tested with domain = core.windows.net, and Cloud Storage (not Developer Storage).

Once setup, select your storage account from the dropdown and click "Open Account". This will populate the 
list of tables from your storage account. Select the table you want to fetch log data from, set your Start 
and End Date/Time (it will always round to the hour since that's how Azure logs are partitioned), and click
Fetch.

This will async fetch the data and create a new window/view on your log. Each window has its own view, so
you can "Fetch Data" into as many windows as you want. (Each window can filter on its own, but there's only
one copy of the data ever downloaded).
