## Our Query

```sql
SELECT c1,
       c2,
       _c3
FROM   t1
WHERE  c1 = 5.23
       AND c2 >= 2.6
        OR _c3 < 2
```

## Components

| Category        | Definition  |
| ------------- | ------------- |
| digit      | [0-9] |
| letter      | [a-zA-Z]      |
| int | \<digit\>+      |
| float | \<digit\>+.\<digit\>+      |
| identifier | (\<underline\>)\*\<letter\>(\<underline\>\<letter>\<digit\>)*      |
| keyword | SELECT &#124; FROM &#124; WHERE &#124; AND &#124; OR | 
| operator | = &#124; < &#124; > &#124; >= |
| comma | , |
| underline | _ |

##### Map:
```
[] : range
+  : at least one
*  : zero or more
|  : or
() : group
<> : category
```

## EBNF (Extended Backus–Naur Form):

```
query
    : 


```





## Rules

| Grammar notation	  | Code representation  | 
|---|---|
|Terminal|Code to match and consume a token|
|Non-Terminal| Call to the related rule's function|
| `|`  | `if` or `switch` statement|
|`*` or `+`| `while` or `for` loop|
|`?`|`if` statement|


## Our Classes & Methods

