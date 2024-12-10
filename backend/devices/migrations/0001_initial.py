# Generated by Django 3.2.19 on 2024-12-10 22:26

from django.db import migrations, models


class Migration(migrations.Migration):

    initial = True

    dependencies = [
    ]

    operations = [
        migrations.CreateModel(
            name='Device',
            fields=[
                ('id', models.AutoField(auto_created=True, primary_key=True, serialize=False, verbose_name='ID')),
                ('name', models.CharField(max_length=100, verbose_name='Device Name')),
                ('ip_famo', models.GenericIPAddressField(blank=True, null=True, verbose_name='FAMO Network IP')),
                ('ip_if', models.GenericIPAddressField(blank=True, null=True, verbose_name='IF Network IP')),
                ('mac', models.CharField(max_length=17, verbose_name='MAC Address')),
                ('device_type', models.CharField(max_length=50, verbose_name='Device Type')),
                ('location', models.CharField(max_length=100, verbose_name='Location')),
                ('description', models.TextField(blank=True, null=True, verbose_name='Description')),
                ('username', models.CharField(blank=True, max_length=100, null=True, verbose_name='Username')),
                ('comment', models.TextField(blank=True, null=True, verbose_name='Additional Comment')),
                ('created_at', models.DateTimeField(auto_now_add=True, verbose_name='Created At')),
                ('updated_at', models.DateTimeField(auto_now=True, verbose_name='Updated At')),
            ],
        ),
    ]
