 Goals and Constraints

Snitz Forums is designed to run using any of a number of different databases, and a few variations in schema (notably, differences in column name prefixes).

To deal with this, the first version of the code was written using the data server-agnotic classes of Micrososoft Enterprise Library, with most of the work being done in Sql stored procedures. The code was written using version 2.0 of ENterprise Library, and has since been compiled (without modification) using version 3.0 of the library. I've written one set of stored procedures for my particular setup, which I believe is the most popular (Sql Server 2000). In theory, all that needs to be done to accomidate other platforms is to adapt the stored procedures. I'm looking to people with different setups for help with the conversion and testing.

The 2nd release of the code eliminates the store procedures and moves the data access in the code using LINQ, so all the business logic is in one place. Please use the discussion forum to express your opinions on this design. 

Also, both releases have many functions stubbed out, mostly those dealing with retrieving passwords (since hashed passwords cannot be retrieved), but also some (such as CreateUser) where Snitz Forums already provides a user interface, and where the MembershipProvider version is inadeque for use with Snitz Forums. Community feedback an which, if any, of these should be fully implemented.
