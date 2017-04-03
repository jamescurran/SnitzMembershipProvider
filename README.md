ASP.NET 2.0 Membership and Role Providers which use a Snitz Forums (http://www.snitz.com/) database as the backing 
store. This allows a website currently using Snitz forums, to leverage the existing membership database when added new features 
to the website.

Snitz Forums 2000 (http://forum.snitz.com/ is a free web-site forum package written in Classic ASP, which uses it's own proprietary schema for member authentication and authorization. It is not directly compatible which the database schema used by the standard membership providers that come with ASP.Net v2.0. To build features onto the site which use ASP.Net's integrated Membership API, a webmaster is given the choice of
 - Dumping the membership list (requiring all users to register again)
 - Manually transforming the data from one schema to the other (but since they use different one-way hashs for storing passwords, all users would have to be assigned new passwords)
 - (Both of the above would also require replacing Snitz forums with a different forum application leading to a similar dilemma of dumping or converting the existing messages.)
 - Alternately, a Snitz membership provider allows continuing the use the same forum software, with a message history and membership accounts (and passwords) remaining in place, while still using the Membership APIs in other areas of the website.
