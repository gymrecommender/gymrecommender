from django.shortcuts import render

# Create your views here.

from django.http import JsonResponse

def helloWorld(request):
    return JsonResponse({"message" : "Hello World"})



# to run everything:
# cd to the directory of myproject in terminal, run:
#     -> source env/bin/activate(macOs i dunno how is it on windows)
#     -> python3 manage.py runserver
#
# copy paste into browser and run: http://127.0.0.1:8000/hello/