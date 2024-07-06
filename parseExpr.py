from sympy.parsing.sympy_parser import parse_expr
from sympy.parsing.sympy_parser import standard_transformations, implicit_multiplication_application, convert_xor

def parse_expression(input):
   transformations = (standard_transformations + (implicit_multiplication_application,convert_xor,))
   return parse_expr(input, transformations=transformations)
