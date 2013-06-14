UrlTracker
==========
The **Url Tracker** for Umbraco.

## About ##
The Url Tracker is used to manage URLs within umbraco. It automatically tracks URL changes, for instance when a node is renamed, and makes sure the old URL will redirect to the new location. This is great for SEO and great for people visiting your website via this old URL. Search engines will update the indexed URL and people won't visit the old, broken URL.<br />
You can also create your own redirects, based on a simple URL or using a Regex pattern. You can redirect to an existing node or a manually entered URL. This is great for migrating existing indexed URLs to your new website!

## Features ##
- Keeps track of **URL changes** (node gets renamed, moved or the umbracoUrlName property changes)
- Keeps track of **not found (404)** requests, so you can easily create redirects for them
- Keeps track of **removed content** and responds with **410 Gone** when one requests it
- **Redirects requests** for old URLs to their new location
- **Create** your own URL redirects, based on a simple URL or a **Regex** pattern. You can redirect to an **existing node** or a manually entered URL.
- Create **permanent (301)** or **temporary (302)** redirects
- Supports all extensions, including **.php** and **.html**
- Supports all kinds of **query string** options, like matching a query string and pass through the request query string
- Supports **multiple websites** in a single umbraco instance

## Roadmap ##
- Datatype
- Better validation (already existing etc.)
- Filtering
- Searching
- Regex capturing groups
- Translation support
- Support UrlRewriting if possible
- SQLCE/Azure support (if it doesn't work yet)

## Upgrading from v1 (301 URL Tracker) ##
1. Back-up the existing infocaster301 database table (schema **and** data)
2. Uninstall the old package
3. When the database table is removed, restore it by using the script from step 1
4. Install version 2; the Url Tracker
5. The installation wizard will be able to migrate the existing data
6. If the migration succeeded, you can delete the old infocaster301 database

## Upgrading from v2 ##
1. Uninstall the old package (no data will be lost)
2. Install the new package

## Uninstalling ##
You can uninstall the Url Tracker by removing the package. The database table will not get deleted! If you'd like to remove the database table too, you should do it manually.