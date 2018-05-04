class ShortJson::Parser

rule

program: value_list_p

value_list_p: value_list
            |            { result = [] }

value_list: value ',' value_list { result = val[2].unshift(val[0]) }
          | value                { result = [val[0]] }

value: literal '(' args ')' { result = make_args([[nil,val[0]]]+val[2]) }
     | literal
	 | '(' args ')'         { result = make_args(val[1]) }
     | '[' value_list_p ']' { result = val[1] }

args: key_value_list
    |                { result = [] }

/*key_value_list_p: key_value_list
	            |                { [] }
*/

key_value_list: key_value ',' key_value_list { result = [val[0]]+val[2] }
              | key_value                    { result = [val[0]] }

key_value: ident ':' value { result = [val[0], val[2]] }
         | value           { result = [nil, val[0]] }

/* value_list_p: value_list | { [] } */

/* value_list: value ',' value_list { result = [val[0]]+val[2] }
   | value { result = [val[0]] } */

ident: STRING

literal: NUMBER
       | STRING
